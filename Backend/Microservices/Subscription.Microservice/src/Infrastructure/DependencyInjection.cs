using System;
using Application.Common.Interfaces;
using Infrastructure.Consumers;
using Infrastructure.Context;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>(options =>
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "subscription_microservice";
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "password";
            var sslMode = Environment.GetEnvironmentVariable("DATABASE_SSLMODE") ?? "Prefer";

            var connectionString =
                $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode={sslMode}";

            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();

        // Configure MassTransit with RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentSuccessConsumer>();
            x.AddConsumer<GetSubscriptionPriceConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
                var rabbitMqPort = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");
                var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
                var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";

                cfg.Host(rabbitMqHost, (ushort)rabbitMqPort, "/", h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPassword);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
