using SharedLibrary.Common.ResponseModel;

namespace Application.Categories;

public static class CategoryErrors
{
    public static Error NotFound(Guid categoryId) =>
        new("Category.NotFound", $"Category with id '{categoryId}' was not found.");

    public static Error DuplicateName(string name) =>
        new("Category.DuplicateName", $"A category with name '{name}' already exists.");
}
