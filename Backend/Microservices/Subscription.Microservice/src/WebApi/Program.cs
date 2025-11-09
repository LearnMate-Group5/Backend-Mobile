using System;
using Application;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.OpenApi.Models;
using Serilog;
using SharedLibrary.Authentication;
using SharedLibrary.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? string.Empty;
if (!string.IsNullOrWhiteSpace(solutionDirectory))
{
    DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                               ForwardedHeaders.XForwardedProto | 
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// CORS Configuration
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Subscription Service API",
        Version = "v1",
        Description = "Subscription plan management microservice."
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Paste a valid JWT access token (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddAuthorization();

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

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

// 3) Logging
app.UseSerilogRequestLogging();

// 4) Only redirect to HTTPS if scheme is already corrected by step #2
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 5) Swagger (server URL patched via PreSerialize)
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
    c.SwaggerEndpoint("./v1/swagger.json", "Subscription Service API V1");
    c.RoutePrefix = "swagger";
});

// Enable CORS
app.UseCors(CorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

// 6) Auth pipeline
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

// ---------- endpoints ----------
app.MapControllers();

logger.LogInformation(
    "Subscription microservice started on port {Port}",
    Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "5008");

app.Run();
