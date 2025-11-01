using System;

namespace Domain.Entities;

public class BookChapter
{
    public Guid ChapterId { get; set; }

    public Guid BookId { get; set; }

    public int PageIndex { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string? UpdatedBy { get; set; }

    public Book Book { get; set; } = null!;
}
