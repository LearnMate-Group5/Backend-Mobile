using Application;
using Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Webhook API",
        Version = "v1",
        Description = "Proxy microservice for AI upload and translate workflow."
    });
});

builder.Services.AddAuthorization();

builder.Services
    .AddApplication()
    .AddInfrastructure();

var app = builder.Build();

app.MapGet("/health", () => new { status = "ok" });
app.MapGet("/api/health", () => new { status = "ok" });

app.UseSwagger();
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

app.UseAuthorization();

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("AI microservice started on port {Port}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "5001");

app.Run();
