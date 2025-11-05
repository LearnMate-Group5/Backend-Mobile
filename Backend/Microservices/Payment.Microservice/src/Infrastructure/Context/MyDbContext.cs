using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "paymentdb1";
            var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "password";
            var sslMode = Environment.GetEnvironmentVariable("DATABASE_SSLMODE") ?? "Require";

            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode={sslMode}";
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_transaction_pkey");
            entity.ToTable("payment_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            
            // Common fields
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .HasColumnName("user_id");
            entity.Property(e => e.OrderId)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("order_id");
            entity.Property(e => e.RequestId)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("request_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentGateway)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("payment_gateway");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("status");
            entity.Property(e => e.OrderInfo)
                .HasMaxLength(500)
                .HasColumnName("order_info");
            entity.Property(e => e.ResultCode)
                .HasColumnName("result_code");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("message");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");
            
            // ZaloPay specific fields
            entity.Property(e => e.AppTransId)
                .HasMaxLength(100)
                .HasColumnName("app_trans_id");
            entity.Property(e => e.ZpTransToken)
                .HasMaxLength(200)
                .HasColumnName("zp_trans_token");
            entity.Property(e => e.QrCode)
                .HasColumnName("qr_code");
            
            // MoMo specific fields
            entity.Property(e => e.MomoTransId)
                .HasMaxLength(100)
                .HasColumnName("momo_trans_id");
            entity.Property(e => e.PayType)
                .HasMaxLength(50)
                .HasColumnName("pay_type");
            
            // Callback data
            entity.Property(e => e.CallbackData)
                .HasColumnName("callback_data");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
