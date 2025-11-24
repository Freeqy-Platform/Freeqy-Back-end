using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Entities;

namespace Freeqy_APIs.Services;

public class ProjectService(ApplicationDbContext dbContext, IMapper mapper) : IProjectService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ProjectListResponse>> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var projectList = await _dbContext.Set<Projects>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .Include(p => p.ProjectMembers)
            .Include(p => p.Technologies)
            .ProjectToType<ProjectListItemResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(new ProjectListResponse(projectList));
    }
}
