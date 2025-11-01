using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Context;

public class DesignTimeMyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();

        var connectionString = ExtractConnectionString(args);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "book_microservice";
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "password";
            var sslMode = Environment.GetEnvironmentVariable("DATABASE_SSLMODE") ?? "Prefer";

            connectionString =
                $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode={sslMode}";
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new MyDbContext(optionsBuilder.Options);
    }

    private static string? ExtractConnectionString(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            return null;
        }

        foreach (var arg in args)
        {
            if (arg.StartsWith("--connection-string=", StringComparison.OrdinalIgnoreCase))
            {
                var value = arg.Substring("--connection-string=".Length);
                return value.Trim('"');
            }
        }

        return null;
    }
}
