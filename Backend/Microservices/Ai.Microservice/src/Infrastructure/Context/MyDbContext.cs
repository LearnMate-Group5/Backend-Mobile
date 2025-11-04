using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiFile> AiFiles { get; set; } = null!;

    public virtual DbSet<AiSession> AiSessions { get; set; } = null!;

    public virtual DbSet<AiSessionMessage> AiSessionMessages { get; set; } = null!;

    public virtual DbSet<TextToSpeechLink> TextToSpeechLinks { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "ai_microservice";
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "password";
            var sslMode = Environment.GetEnvironmentVariable("DATABASE_SSLMODE") ?? "Prefer";

            var connectionString =
                $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode={sslMode}";

            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiFile>(entity =>
        {
            entity.ToTable("ai_files");

            entity.HasKey(e => e.FileId);

            entity.Property(e => e.FileId)
                .HasColumnName("file_id");

            entity.Property(e => e.FileName)
                .HasMaxLength(260)
                .HasColumnName("file_name");

            entity.Property(e => e.OcrContent)
                .HasColumnName("ocr_content");

            entity.Property(e => e.TranslatedContent)
                .HasColumnName("translated_content");

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .HasColumnName("user_id");

            entity.Property(e => e.CreatedDate)
                .HasColumnName("created_date")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<int>();

            entity.Property(e => e.CurrentContent)
                .HasColumnName("current_content");
        });

        modelBuilder.Entity<AiSessionMessage>(entity =>
        {
            entity.ToTable("ai_session_messages");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.SessionId)
                .HasMaxLength(200)
                .HasColumnName("session_id");

            entity.Property(e => e.Message)
                .HasColumnName("message")
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<AiSession>(entity =>
        {
            entity.ToTable("ai_sessions");

            entity.HasKey(e => e.SessionId);

            entity.Property(e => e.SessionId)
                .HasMaxLength(200)
                .HasColumnName("session_id");

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .HasColumnName("user_id");

            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.Property(e => e.CreatedDate)
                .HasColumnName("created_date")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.LastActivityDate)
                .HasColumnName("last_activity_date")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<TextToSpeechLink>(entity =>
        {
            entity.ToTable("text_to_speech_links");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasMaxLength(128)
                .HasColumnName("user_id");

            entity.Property(e => e.UniqueId)
                .HasMaxLength(255)
                .HasColumnName("unique_id");

            entity.Property(e => e.CreatedDate)
                .HasColumnName("created_date")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_text_to_speech_links_user_id");

            entity.HasIndex(e => e.UniqueId)
                .IsUnique()
                .HasDatabaseName("idx_text_to_speech_links_unique_id");
        });

    }
}
