using Application;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models; // <<— for JWT in Swagger
using Serilog;
using SharedLibrary.Configs;
using SharedLibrary.Middleware;
using SharedLibrary.Migrations;
using SharedLibrary.Utils;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Linq;

var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "";
if (solutionDirectory != null)
{
    DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
}

// Update appsettings files before building the app
AutoScaffold.UpdateAppSettingsFile("appsettings.json", "default");
AutoScaffold.UpdateAppSettingsFile("appsettings.Development.json", "default");

var builder = WebApplication.CreateBuilder(args);
const string AutoApplyMigrationsEnvVar = "AUTO_APPLY_MIGRATIONS";
var autoApplySetting = Environment.GetEnvironmentVariable(AutoApplyMigrationsEnvVar);
var shouldAutoApplyMigrations =
    bool.TryParse(autoApplySetting, out var parsedAutoApply) && parsedAutoApply;

if (!shouldAutoApplyMigrations)
{
    builder.Services.Replace(ServiceDescriptor.Scoped<IMigrator, NoOpMigrator>());
}

var environment = builder.Environment;
const string CorsPolicyName = "AllowFrontend";
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var allowedCorsOrigins = (configuredOrigins ?? Array.Empty<string>())
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (allowedCorsOrigins.Length == 0)
{
    allowedCorsOrigins = new[] { "http://localhost:5173" };
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure forwarded headers for CloudFront/ALB support
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(allowedCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// --- Swagger with JWT "Authorize" button ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User API",
        Version = "v1"
    });

    // Add the JWT bearer definition so Swagger UI shows the "Authorize" button
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT token here (no need to type 'Bearer ').",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    // Apply the bearer auth globally to all operations.
    // (If you want to require it only on [Authorize] endpoints, add an IOperationFilter that checks attributes.)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddAuthorization();

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services.ConfigureOptions<DatabaseConfigSetup>();
builder.Services.AddDbContext<MyDbContext>((serviceProvider, options) =>
{
    var databaseConfig = serviceProvider.GetRequiredService<IOptions<DatabaseConfig>>().Value;
    options.UseNpgsql(databaseConfig.ConnectionString, actions =>
    {
        actions.EnableRetryOnFailure(databaseConfig.MaxRetryCount);
        actions.CommandTimeout(databaseConfig.CommandTimeout);
    });

    if (environment.IsDevelopment())
    {
        options.EnableDetailedErrors(databaseConfig.EnableDetailedErrors);
        options.EnableSensitiveDataLogging(databaseConfig.EnableSensitiveDataLogging);
    }
});

builder.Services
    .AddApplication()
    .AddInfrastructure();

var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];
if (!string.IsNullOrWhiteSpace(firebaseCredentialsPath))
{
    var absolutePath = Path.IsPathRooted(firebaseCredentialsPath)
        ? firebaseCredentialsPath
        : Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, firebaseCredentialsPath));

    try
    {
        if (!File.Exists(absolutePath))
        {
            Console.WriteLine($"[Firebase] Credential file not found at '{absolutePath}'. Firebase login will be disabled.");
        }
        else if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(absolutePath)
            });
            Console.WriteLine("[Firebase] Admin SDK initialized using configured credentials file.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Firebase] Failed to initialize Firebase Admin SDK: {ex.Message}");
    }
}
else
{
    Console.WriteLine("[Firebase] 'Firebase:CredentialsPath' is not configured. Firebase login will be disabled.");
}

var app = builder.Build();

if (shouldAutoApplyMigrations)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        dbContext.Database.Migrate();
        app.Logger.LogInformation("EF Core migrations applied successfully at startup.");
        // Ensure password_reset_requests table exists in case it was removed while migration is recorded
        try
        {
            var ensureSql = @"DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = 'password_reset_requests'
    ) THEN
        CREATE TABLE IF NOT EXISTS public.password_reset_requests (
            password_reset_request_id uuid NOT NULL DEFAULT gen_random_uuid(),
            user_id uuid NOT NULL,
            token character varying(200) NOT NULL,
            otp_hash character varying(255) NOT NULL,
            expires_at timestamp with time zone NOT NULL,
            created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
            used boolean NOT NULL,
            CONSTRAINT password_reset_requests_pkey PRIMARY KEY (password_reset_request_id),
            CONSTRAINT password_reset_requests_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users (user_id) ON DELETE CASCADE
        );
        CREATE UNIQUE INDEX IF NOT EXISTS password_reset_requests_token_key ON public.password_reset_requests (token);
        CREATE INDEX IF NOT EXISTS IX_password_reset_requests_user_id ON public.password_reset_requests (user_id);
    END IF;
END $$;";

            dbContext.Database.ExecuteSqlRaw(ensureSql);
            app.Logger.LogInformation("Ensured password_reset_requests table exists.");
        }
        catch (Exception innerEx)
        {
            app.Logger.LogWarning(innerEx, "Failed to ensure password_reset_requests table exists.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to apply EF Core migrations at startup.");
        throw;
    }
}
else
{
    app.Logger.LogInformation("Automatic EF Core migrations are disabled. Set {EnvVar}=true to enable.", AutoApplyMigrationsEnvVar);
}
// Health check endpoints
app.MapGet("/health", () => new { status = "ok" });
app.MapGet("/api/health", () => new { status = "ok" });

// Use forwarded headers (must be before UseHttpsRedirection and other middleware)
app.UseForwardedHeaders();

// Always enable Swagger in microservices; gateway controls external exposure
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var useHttps = Environment.GetEnvironmentVariable("USE_HTTPS")?.ToLowerInvariant() == "true";
        
        if (useHttps)
        {
            var host = httpReq.Headers["X-Forwarded-Host"].FirstOrDefault() 
                       ?? httpReq.Headers["Host"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(host))
            {
                swagger.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = $"https://{host}" }
                };
            }
        }
    });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API V1");
    c.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.UseSerilogRequestLogging();

// Only use HTTPS redirection in production with proper certificates
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);

// Your custom JWT middleware validates token and sets HttpContext.User
app.UseMiddleware<JwtMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("User microservice started on port {Port}",
    Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "5002");

app.Run();
