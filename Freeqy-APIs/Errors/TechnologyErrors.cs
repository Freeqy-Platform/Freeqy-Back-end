namespace Freeqy_APIs.Errors;

public static class TechnologyErrors
{
    public static readonly Error DuplicateName =
        new("Technology.DuplicateName", "Another technology with the same name is already exists", 
            StatusCodes.Status409Conflict);
    
    public static readonly Error NotFound =
        new("Technology.NotFound", "Technology not found", StatusCodes.Status404NotFound);
}