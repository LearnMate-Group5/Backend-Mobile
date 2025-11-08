using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Application;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SharedLibrary.Configs;
using SharedLibrary.Middleware;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;

// Add early logging to debug environment variables
var tempLogger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
tempLogger.LogInformation("=== Database Environment Variables Debug ===");
tempLogger.LogInformation("DATABASE_HOST: {Host}", Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "NOT SET");
tempLogger.LogInformation("DATABASE_PORT: {Port}", Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "NOT SET");
tempLogger.LogInformation("DATABASE_NAME: {Database}", Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "NOT SET");
tempLogger.LogInformation("DATABASE_USERNAME: {Username}", Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "NOT SET");
tempLogger.LogInformation("DATABASE_PASSWORD: {HasPassword}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_PASSWORD")) ? "SET" : "NOT SET");
tempLogger.LogInformation("ASPNETCORE_ENVIRONMENT: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "NOT SET");
tempLogger.LogInformation("=== End Debug Info ===");

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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

// Configure database
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

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// --- Swagger configuration ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment API",
        Version = "v1",
        Description = "API for managing payments with ZaloPay and MoMo"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication to Swagger (HTTP Bearer scheme)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter only the JWT token. The 'Bearer' prefix will be added automatically.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Health check endpoints
app.MapGet("/health", () => new { status = "ok" });
app.MapGet("/api/health", () => new { status = "ok" });

// Debug endpoint to check headers
app.MapGet("/debug/headers", (HttpContext context) =>
{
    var headers = context.Request.Headers
        .ToDictionary(h => h.Key, h => h.Value.ToString());
    return Results.Ok(new
    {
        headers,
        scheme = context.Request.Scheme,
        host = context.Request.Host.ToString(),
        path = context.Request.Path.ToString()
    });
});

// ---------- middleware order matters ----------

// 1) Forwarded headers FIRST
app.UseForwardedHeaders();

// 2) Respect CloudFront viewer scheme (HTTPS at the edge)
app.Use((ctx, next) =>
{
    var cfProto = ctx.Request.Headers["CloudFront-Forwarded-Proto"].ToString();
    if (string.Equals(cfProto, "https", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Request.Scheme = "https";
        ctx.Request.IsHttps = true;
    }
    return next();
});

// 3) Swagger (server URL patched via PreSerialize)
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var proto = httpReq.Headers["CloudFront-Forwarded-Proto"].FirstOrDefault()
                    ?? httpReq.Headers["X-Forwarded-Proto"].FirstOrDefault()
                    ?? httpReq.Scheme;

        var host = httpReq.Headers["Host"].FirstOrDefault()
                   ?? httpReq.Host.Value;

        if (!string.IsNullOrEmpty(proto) && !string.IsNullOrEmpty(host))
        {
            swagger.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = $"{proto}://{host}" }
            };
        }
    });
});

app.UseSwaggerUI(c =>
{
    // Relative path is safer behind proxies/CDNs
    c.SwaggerEndpoint("./v1/swagger.json", "Payment API V1");
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

// 4) Auth pipeline
app.UseMiddleware<JwtMiddleware>();

// ---------- endpoints ----------
app.UseRouting();
app.MapControllers();

app.Run();
