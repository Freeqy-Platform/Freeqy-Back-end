using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Entities;

namespace Freeqy_APIs.Services;

public class ProjectService(ApplicationDbContext dbContext, IMapper mapper) : IProjectService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ProjectListResponse>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var projectsQuery = _dbContext.Set<Projects>()
            .Include(p => p.Category)
            .Include(p => p.Owner)
            // .Include(p => p.ProjectTechnologies)
            //     .ThenInclude(pt => pt.technology)
            // .Include(p => p.ProjectMembers)
            .AsQueryable();

        // Use Mapster projection to DTO
        var projectList = await projectsQuery
            .ProjectToType<ProjectListItemResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(new ProjectListResponse(projectList));
    }
}
