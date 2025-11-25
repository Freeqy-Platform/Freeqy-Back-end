using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Entities;

namespace Freeqy_APIs.Services;

public class ProjectService(ApplicationDbContext dbContext) : IProjectService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    public async Task<Result<ProjectListResponse>> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var projectList = await _dbContext.Projects
            .AsNoTracking()
            .ProjectToType<ProjectListItemResponse>() 
            .ToListAsync(cancellationToken);
        return Result.Success(new ProjectListResponse(projectList));
    }


}
