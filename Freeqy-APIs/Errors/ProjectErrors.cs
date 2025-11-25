namespace Freeqy_APIs.Errors;

public class ProjectErrors
{
    public static readonly Error DuplicateName =
        new("Project.DuplicateName", "Another project with the same name is already exists", 
            StatusCodes.Status409Conflict);
    
    public static readonly Error NotFound =
        new("Project.NotFound", "Project not found", StatusCodes.Status404NotFound);
    public static readonly Error Forbidden =
        new("Project.Forbidden", "You do not have permission to modify this project", 
            StatusCodes.Status403Forbidden);
    public static readonly Error NotDeleted =
        new("Project.NotDeleted", "This project has not been deleted", 
            StatusCodes.Status400BadRequest);
}