using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedLibrary.Configs
{
    public class EnvironmentConfig
    {
        public string DatabaseHost => Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "pg-2-database25812.g.aivencloud.com";
        public int DatabasePort => int.TryParse(Environment.GetEnvironmentVariable("DATABASE_PORT"), out var port) ? port : 19217;
        public string DatabaseName => Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "userdb";
        public string DatabaseUser => Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "avnadmin";
        public string DatabasePassword => Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "AVNS_vsIotPLRrxJUhcJlM0m";
        public string DatabaseProvider => Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? "postgres";
        
        // RabbitMQ Cloud Configuration (priority)
        public string? RabbitMqUrl => Environment.GetEnvironmentVariable("RABBITMQ_URL");
        
        // RabbitMQ Local Configuration (fallback)
        public string RabbitMqHost => Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbit-mq";
        public int RabbitMqPort  => int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672;
        public string RabbitMqUser => Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "username";
        public string RabbitMqPassword => Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "password";
        
        // Helper property to determine if using cloud RabbitMQ
        public bool IsRabbitMqCloud => !string.IsNullOrEmpty(RabbitMqUrl);

        public string RedisHost => Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis";
        public string RedisPassword => Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "default";
        public int RedisPort => int.TryParse(Environment.GetEnvironmentVariable("REDIS_PORT"), out var port) ? port : 6379;
    }
} 
