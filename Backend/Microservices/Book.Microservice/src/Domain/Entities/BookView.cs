using System;

namespace Domain.Entities;

public class BookView
{
    public Guid BookViewId { get; set; }

    public Guid BookId { get; set; }

    public string? ViewerId { get; set; }

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
}

