using System;
using Application.Common.Interfaces;
using Infrastructure.Context;
using Infrastructure.Options;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    private const string EndpointEnvKey = "AiWebhook__Endpoint";
    private const string TimeoutEnvKey = "AiWebhook__TimeoutSeconds";
    private const string LegacyEndpointEnvKey = "AI_WEBHOOK_ENDPOINT";
    private const string LegacyTimeoutEnvKey = "AI_WEBHOOK_TIMEOUT_SECONDS";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>(options =>
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "ai_microservice";
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "password";
            var sslMode = Environment.GetEnvironmentVariable("DATABASE_SSLMODE") ?? "Prefer";

            var connectionString =
                $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode={sslMode}";

            options.UseNpgsql(connectionString);
        });

        services
            .AddOptions<AiWebhookOptions>()
            .Configure(options =>
            {
                options.Endpoint = GetEnv(EndpointEnvKey, LegacyEndpointEnvKey) ?? string.Empty;

                var timeoutValue = GetEnv(TimeoutEnvKey, LegacyTimeoutEnvKey);
                if (int.TryParse(timeoutValue, out var timeout) && timeout > 0)
                {
                    options.TimeoutSeconds = timeout;
                }
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Endpoint),
                $"{EndpointEnvKey} environment variable is required.");

        services.AddHttpClient<IAiWebhookClient, HttpAiWebhookClient>();
        services.AddScoped<IAiFileRepository, AiFileRepository>();

        return services;
    }

    private static string? GetEnv(params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
