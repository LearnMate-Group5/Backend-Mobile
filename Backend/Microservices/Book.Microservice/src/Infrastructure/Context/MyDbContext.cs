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

    public virtual DbSet<Book> Books { get; set; } = null!;
    public virtual DbSet<BookChapter> BookChapters { get; set; } = null!;
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public virtual DbSet<BookCategory> BookCategories { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "book_microservice";
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
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("books");

            entity.HasKey(e => e.BookId);

            entity.Property(e => e.BookId)
                .HasColumnName("book_id");

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Author)
                .HasColumnName("author")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .IsRequired();

            entity.Property(e => e.ImageBase64)
                .HasColumnName("image_base64")
                .HasColumnType("text");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(128);

            entity.HasIndex(e => e.Title)
                .IsUnique()
                .HasDatabaseName("ux_books_title");

            entity.HasMany(e => e.Chapters)
                .WithOne(chapter => chapter.Book)
                .HasForeignKey(chapter => chapter.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.BookCategories)
                .WithOne(bookCategory => bookCategory.Book)
                .HasForeignKey(bookCategory => bookCategory.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("ux_categories_name");
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.ToTable("book_categories");

            entity.HasKey(e => new { e.BookId, e.CategoryId });

            entity.Property(e => e.BookId)
                .HasColumnName("book_id");

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id");

            entity.HasOne(e => e.Book)
                .WithMany(book => book.BookCategories)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(category => category.BookCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookChapter>(entity =>
        {
            entity.ToTable("book_chapters");

            entity.HasKey(e => e.ChapterId);

            entity.Property(e => e.ChapterId)
                .HasColumnName("chapter_id");

            entity.Property(e => e.BookId)
                .HasColumnName("book_id")
                .IsRequired();

            entity.Property(e => e.PageIndex)
                .HasColumnName("page_index")
                .IsRequired();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Content)
                .HasColumnName("content")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(128);

            entity.HasIndex(e => new { e.BookId, e.PageIndex })
                .IsUnique()
                .HasDatabaseName("ux_book_chapters_page_index");
        });
    }
}

