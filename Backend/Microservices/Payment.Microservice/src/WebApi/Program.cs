using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Application;
using Infrastructure;
using Infrastructure.Context;
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

// Add JWT middleware
app.UseMiddleware<JwtMiddleware>();

// Always enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API V1");
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

app.UseRouting();
app.MapControllers();

app.Run();
