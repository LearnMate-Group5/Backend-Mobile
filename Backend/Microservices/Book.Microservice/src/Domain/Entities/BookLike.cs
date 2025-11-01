using System;

namespace Domain.Entities;

public class BookLike
{
    public Guid BookLikeId { get; set; }

    public Guid BookId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Book Book { get; set; } = null!;
}

