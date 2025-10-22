using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Context;

public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        string? connectionString = null;
        foreach (var arg in args)
        {
            const string prefix = "--connection-string=";
            if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                connectionString = arg.Substring(prefix.Length).Trim('"');
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "pg-2-database25812.g.aivencloud.com";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "19217";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "userdb";
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "avnadmin";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "AVNS_vsIotPLRrxJUhcJlM0m";
            var sslMode = Environment.GetEnvironmentVariable("DATABASE_SSLMODE") ?? "Require";
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode={sslMode}";
        }

        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        optionsBuilder.UseNpgsql(connectionString!);
        return new MyDbContext(optionsBuilder.Options);
    }
}

