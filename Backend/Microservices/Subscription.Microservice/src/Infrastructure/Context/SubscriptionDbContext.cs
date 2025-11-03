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

    public virtual DbSet<Subscription> Subscriptions { get; set; } = null!;
    public virtual DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "subscription_microservice";
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
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");

            entity.HasKey(e => e.SubscriptionId);

            entity.Property(e => e.SubscriptionId)
                .HasColumnName("subscription_id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.OriginalPrice)
                .HasColumnName("original_price")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            entity.Property(e => e.Discount)
                .HasColumnName("discount")
                .HasColumnType("numeric(9,2)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(e => new { e.Name, e.Type })
                .IsUnique()
                .HasDatabaseName("ux_subscriptions_name_type");

            entity.HasMany(e => e.UserSubscriptions)
                .WithOne(userSubscription => userSubscription.Subscription)
                .HasForeignKey(userSubscription => userSubscription.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("user_subscriptions");

            entity.HasKey(e => e.UserSubscriptionId);

            entity.Property(e => e.UserSubscriptionId)
                .HasColumnName("user_subscription_id");

            entity.Property(e => e.SubscriptionId)
                .HasColumnName("subscription_id")
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Current");

            entity.Property(e => e.SubscribedAt)
                .HasColumnName("subscribed_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExpiredAt)
                .HasColumnName("expired_at")
                .HasColumnType("timestamp with time zone");

            entity.HasOne(e => e.Subscription)
                .WithMany(subscription => subscription.UserSubscriptions)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SubscriptionId, e.UserId })
                .HasDatabaseName("ix_user_subscriptions_subscription_user");
        });
    }
}
