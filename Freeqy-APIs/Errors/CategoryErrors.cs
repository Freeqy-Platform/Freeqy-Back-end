namespace Freeqy_APIs.Errors;

public static class CategoryErrors
{
    public static readonly Error DuplicateName =
        new("Category.DuplicateName", "Another category with the same name is already exists", 
            StatusCodes.Status409Conflict);
    
    public static readonly Error NotFound =
        new("Category.NotFound", "Category not found", StatusCodes.Status404NotFound);
}