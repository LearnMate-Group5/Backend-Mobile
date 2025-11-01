using Application;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.OpenApi.Models;
using Serilog;
using SharedLibrary.Authentication;
using SharedLibrary.Middleware;
using Microsoft.EntityFrameworkCore;

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
        Title = "Book Service API",
        Version = "v1",
        Description = "Catalog management microservice for books."
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

        EnsureBooksImageColumn(dbContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.MapGet("/health", () => new { status = "ok" });
app.MapGet("/api/health", () => new { status = "ok" });

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Book Service API V1");
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

logger.LogInformation(
    "Book microservice started on port {Port}",
    Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "5004");

app.Run();

static void EnsureBooksImageColumn(MyDbContext dbContext, Microsoft.Extensions.Logging.ILogger logger)
{
    const string ensureImageColumnSql =
        """
        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'books'
                  AND column_name = 'image_base64'
            ) THEN
                ALTER TABLE public.books
                ADD COLUMN image_base64 text;

                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'books'
                      AND column_name = 'image_url'
                ) THEN
                    UPDATE public.books
                    SET image_base64 = image_url
                    WHERE image_base64 IS NULL AND image_url IS NOT NULL;
                END IF;
            END IF;
        END $$;
        """;

    try
    {
        dbContext.Database.ExecuteSqlRaw(ensureImageColumnSql);
        logger.LogInformation("Ensured books.image_base64 column exists.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to ensure books.image_base64 column exists.");
    }
}
