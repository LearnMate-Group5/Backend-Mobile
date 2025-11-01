using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Book
{
    public Guid BookId { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ImageBase64 { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string? UpdatedBy { get; set; }

    public ICollection<BookChapter> Chapters { get; set; } = new List<BookChapter>();

    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}
