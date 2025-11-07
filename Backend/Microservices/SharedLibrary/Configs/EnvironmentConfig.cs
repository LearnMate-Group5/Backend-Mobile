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

        // SMTP / Email configuration
        public string? SmtpHost => Environment.GetEnvironmentVariable("SMTP_HOST");
        public int SmtpPort => int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 587;
        public string? SmtpUsername =>
            Environment.GetEnvironmentVariable("SMTP_USERNAME") ??
            Environment.GetEnvironmentVariable("SMTP_USER");
        public string? SmtpPassword =>
            Environment.GetEnvironmentVariable("SMTP_PASSWORD") ??
            Environment.GetEnvironmentVariable("SMTP_PASS");
        public bool SmtpEnableSsl
        {
            get
            {
                var raw = Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ??
                          Environment.GetEnvironmentVariable("SMTP_SECURE");
                return ParseBool(raw, defaultValue: true);
            }
        }
        public string SmtpFromAddress =>
            Environment.GetEnvironmentVariable("SMTP_FROM_ADDRESS") ??
            Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ??
            "no-reply@example.com";
        public string SmtpFromName =>
            Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ??
            Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ??
            "LearnMate Support";

        // Password reset configuration
        public string PasswordResetLinkTemplate => Environment.GetEnvironmentVariable("PASSWORD_RESET_LINK_TEMPLATE") ?? "https://frontend.example/reset-password?token={token}&email={email}";
        public int PasswordResetTokenExpiryMinutes => int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_TOKEN_EXPIRY_MINUTES"), out var minutes) ? minutes : 15;
        public int PasswordResetOtpLength => int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_OTP_LENGTH"), out var length) ? length : 6;
        public string PasswordResetEmailSubject => Environment.GetEnvironmentVariable("PASSWORD_RESET_EMAIL_SUBJECT") ?? "Reset your password";

        private static bool ParseBool(string? value, bool defaultValue = true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
} 
