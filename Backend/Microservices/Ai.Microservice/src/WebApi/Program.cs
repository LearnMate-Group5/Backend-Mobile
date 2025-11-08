using System;
using System.Linq;
using System.Collections.Generic;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Serilog;
using SharedLibrary.Authentication;
using SharedLibrary.Middleware;

var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? string.Empty;
if (!string.IsNullOrWhiteSpace(solutionDirectory))
{
    DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
}

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Forwarded headers (CloudFront/ALB)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    // Trust all proxies on the path; prefer locking ALB SG to CloudFront IPs
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Webhook API",
        Version = "v1",
        Description = "Proxy microservice for AI upload and translate workflow."
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Paste a valid JWT access token (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddAuthorization();

// App & Infra
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

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
    c.SwaggerEndpoint("./v1/swagger.json", "AI Webhook API V1");
    c.RoutePrefix = "swagger";
});

// 6) Auth pipeline
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

// ---------- endpoints ----------

app.MapControllers();

// Health checks
app.MapGet("/health", () => new { status = "ok" });
app.MapGet("/api/health", () => new { status = "ok" });

// Debug headers (helpful while validating CF/ALB forwarding)
app.MapGet("/debug/headers", (HttpContext context) =>
{
    var headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
    return Results.Ok(new
    {
        headers,
        scheme = context.Request.Scheme,
        host = context.Request.Host.ToString(),
        path = context.Request.Path.ToString()
    });
});

// Dev root redirect to Swagger
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("AI microservice started on {Urls}",
    Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:5001");

app.Run();
