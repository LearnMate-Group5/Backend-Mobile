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

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

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
    .AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

app.MapGet("/health", () => new { status = "ok" });
app.MapGet("/api/health", () => new { status = "ok" });

// Use forwarded headers (must be before other middleware)
app.UseForwardedHeaders();

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Webhook API V1");
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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<JwtMiddleware>();

app.UseAuthorization();

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("AI microservice started on port {Port}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "5001");

app.Run();
