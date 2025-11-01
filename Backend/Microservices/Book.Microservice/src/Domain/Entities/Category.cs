using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Category
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}
