using System;
using System.IO;
using SharedLibrary.Utils;
using SharedLibrary.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibrary.Abstractions.UnitOfWork;
using SharedLibrary.Common;
using Infrastructure.Common;
using Infrastructure.Repositories;
using Infrastructure.Services;
using SharedLibrary.Adapters;
using MassTransit;
using Application.Payments.Services;
using Domain.Configs;
using SharedLibrary.Contracts;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISaveChangesUnitOfWork, SaveChangesUnitOfWorkAdapter>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register ZaloPay service
            services.AddHttpClient<IZaloPayService, ZaloPayService>();

            // Register ZaloPay IPN service
            services.AddScoped<IZaloPayIpnService, ZaloPayIpnService>();

            // Register MoMo service
            services.AddHttpClient<IMoMoService, MoMoService>();

            // Register MoMo IPN service
            services.AddScoped<IMoMoIpnService, MoMoIpnService>();

            // Register Subscription service (for RabbitMQ communication)
            services.AddScoped<ISubscriptionService, SubscriptionService>();

            // Register ZaloPay configuration
            services.Configure<ZaloPayConfig>(options =>
            {
                options.AppId = int.TryParse(Environment.GetEnvironmentVariable("ZaloPay__AppId"), out var appId) ? appId : 0;
                options.Key1 = Environment.GetEnvironmentVariable("ZaloPay__Key1") ?? string.Empty;
                options.Key2 = Environment.GetEnvironmentVariable("ZaloPay__Key2") ?? string.Empty;
                options.BaseUrl = Environment.GetEnvironmentVariable("ZaloPay__BaseUrl") ?? "https://sb-openapi.zalopay.vn";
                options.CallbackUrl = Environment.GetEnvironmentVariable("ZaloPay__CallbackUrl") ?? string.Empty;
            });

            // Register configuration
            services.AddSingleton<EnvironmentConfig>();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            using var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<AutoScaffold>>();
            var config = serviceProvider.GetRequiredService<EnvironmentConfig>();
            var scaffold = new AutoScaffold(logger)
                .Configure(
                    config.DatabaseHost,
                    config.DatabasePort,
                    config.DatabaseName,
                    config.DatabaseUser,
                    config.DatabasePassword,
                    config.DatabaseProvider);

            scaffold.UpdateAppSettings();
            string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? string.Empty;
            if (!string.IsNullOrEmpty(solutionDirectory))
            {
                DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
            }

            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                // Register request client for getting subscription prices
                busConfigurator.AddRequestClient<GetSubscriptionPriceRequest>();

                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    if (config.IsRabbitMqCloud)
                    {
                        configurator.Host(config.RabbitMqUrl);
                    }
                    else
                    {
                        configurator.Host(new Uri($"rabbitmq://{config.RabbitMqHost}:{config.RabbitMqPort}/"), h =>
                        {
                            h.Username(config.RabbitMqUser);
                            h.Password(config.RabbitMqPassword);
                        });
                    }

                    configurator.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
