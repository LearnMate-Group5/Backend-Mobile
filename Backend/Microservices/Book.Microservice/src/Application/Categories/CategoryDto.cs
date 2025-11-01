using Domain.Entities;

namespace Application.Categories;

public sealed record CategoryDto(Guid CategoryId, string Name)
{
    public static CategoryDto FromEntity(Category category) =>
        new(category.CategoryId, category.Name);
}
