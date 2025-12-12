using Freeqy_APIs.Contracts.Category;
using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.Technology;
using Freeqy_APIs.Entities;
using CategoryResponse = Freeqy_APIs.Contracts.Category.CategoryResponse;

namespace Freeqy_APIs.Services;

public class ProjectService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : IProjectService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<ProjectListResponse>> GetProjectsAsync(
        ProjectRequestFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = GetActiveProjects()
            .Include(p => p.Owner)
            .Include(p => p.Category)
            .Include(p => p.Technologies)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.OwnerId))
            query = query.Where(p => p.OwnerId == filter.OwnerId);

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(p => p.Category.Name == filter.Category);

        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status.Value);

        if (filter.Visibility.HasValue)
            query = query.Where(p => p.Visibility == filter.Visibility.Value);

        if (!string.IsNullOrWhiteSpace(filter.Tech))
            query = query.Where(p => p.Technologies.Any(t => t.Name == filter.Tech));

        var projectList = await query
            .ProjectToType<ProjectListItemResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(new ProjectListResponse(projectList));
    }

    // Should Review This Service
    public async Task<Result<ProjectListItemResponse>> GetProjectByIdAsync(string id, 
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Owner)
            .Include(p => p.Category)
            .Include(p => p.Technologies)
            .Include(p => p.ProjectMembers)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    
        if (project == null)
            return Result.Failure<ProjectListItemResponse>(ProjectErrors.NotFound);
    
        var response = project.Adapt<ProjectListItemResponse>();
    
        return Result.Success(response);
    }

    public async Task<Result> ChangeProjectStatusAsync(string userId, string id, ChangeProjectStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.FindAsync(id, cancellationToken);
        
        if (project == null) 
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
        {
            return Result.Failure(UserErrors.NoAuthenticate);
        }

        if (project.Status != request.ProjectStatus)
        {
            project.Status = request.ProjectStatus;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
            
        return  Result.Success();
    }

    public async Task<Result<CategoryResponse>> AddCategoryAsync(CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var isExist = await _dbContext.Categories.AnyAsync(c => c.Name == request.Name, cancellationToken);
        
        if (isExist)
            return Result.Failure<CategoryResponse>(CategoryErrors.DuplicateName);
        
        Category category = request.Adapt<Category>();
        
        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success(category.Adapt<CategoryResponse>());
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(string id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories.FindAsync(id, cancellationToken);
        
        if (category == null)
            return Result.Failure<CategoryResponse>(CategoryErrors.NotFound);
        
        return  Result.Success(category.Adapt<CategoryResponse>());
    }
    
    public async Task<Result<TechnologyResponse>> AddTechnologyAsync(TechnologyRequest request, CancellationToken cancellationToken = default)
    {
        var isExist = await _dbContext.Technologies.AnyAsync(c => c.Name == request.Name, cancellationToken);
        
        if (isExist)
            return Result.Failure<TechnologyResponse>(TechnologyErrors.DuplicateName);
        
        var technology = request.Adapt<Technology>();
        
        await _dbContext.Technologies.AddAsync(technology, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success(technology.Adapt<TechnologyResponse>());
    }

    public async Task<Result<TechnologyResponse>> GetTechnologyByIdAsync(string id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Technologies.FindAsync(id, cancellationToken);
        
        if (category == null)
            return Result.Failure<TechnologyResponse>(TechnologyErrors.NotFound);
        
        return  Result.Success(category.Adapt<TechnologyResponse>());
    }

    public async Task<Result<List<TechnologyResponse>>> GetTechnologiesAsync(CancellationToken cancellationToken = default)
    {
        var technologies = await _dbContext
            .Technologies
            .AsNoTracking()
            .ProjectToType<TechnologyResponse>()
            .ToListAsync(cancellationToken);
        
        return Result.Success(technologies);
    }
    
    public async Task<Result<List<CategoryResponse>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext
            .Categories
            .AsNoTracking()
            .ProjectToType<CategoryResponse>()
            .ToListAsync(cancellationToken);
        
        return Result.Success(categories);
    }

    public async Task<Result<ProjectListItemResponse>> AddProjectAsync(string userId, ProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var isExistingProjectName = await _dbContext.Projects.AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (isExistingProjectName) return Result.Failure<ProjectListItemResponse>(ProjectErrors.DuplicateName);
        
        var category = await _dbContext.Categories.FindAsync(request.CategoryId, cancellationToken);
        if (category is null) 
            return Result.Failure<ProjectListItemResponse>(CategoryErrors.NotFound);

        var technologies = await _dbContext.Technologies
            .Where(x => request.TechnologyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (technologies.Count != request.TechnologyIds.Count)
            return Result.Failure<ProjectListItemResponse>(TechnologyErrors.NotFound);
        
        var user = _userManager.FindByIdAsync(userId).Result;
        
        Project project = request.Adapt<Project>();
        project.Category = category;
        project.Technologies = technologies;
        project.Owner =  user!;
        project.OwnerId  = userId; 
        
        await _dbContext.Projects.AddAsync(project, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success(project.Adapt<ProjectListItemResponse>());
    }

    public async Task<Result> UpdateProjectAsync(
          string projectId,
          string userId,
          ProjectRequest request,
          CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
            .Include(p => p.Technologies)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        var isDuplicateName = await GetActiveProjects()
            .AnyAsync(p => p.Name == request.Name && p.Id != projectId, cancellationToken);

        if (isDuplicateName)
            return Result.Failure(ProjectErrors.DuplicateName);

        var category = await _dbContext.Categories.FindAsync(new object[] { request.CategoryId },
            cancellationToken: cancellationToken);
        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        var technologies = await _dbContext.Technologies
            .Where(x => request.TechnologyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (technologies.Count != request.TechnologyIds.Count)
            return Result.Failure(TechnologyErrors.NotFound);

        project.Name = request.Name;
        project.Description = request.Description;
        project.CategoryId = request.CategoryId;
        project.Category = category;
        project.UpdatedAt = DateTime.UtcNow;
        project.Status = request.Status;
        project.Visibility = request.Visibility;
        project.EstimatedTime = request.EstimatedTime;

        project.Technologies.Clear();
        project.Technologies = technologies;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    
    public async Task<Result> DeleteProjectAsync(string projectId, string userId,
        CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        project.DeletedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RestoreProjectAsync(string projectId, string userId,
    CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        if (!project.IsDeleted)
            return Result.Failure(ProjectErrors.NotDeleted);

        project.DeletedAt = null;
        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
    public async Task<Result> ChangeProjectVisibilityAsync(string projectId, string userId, CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
          .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        if (project.Visibility == ProjectVisibility.Public)
            project.Visibility = ProjectVisibility.Private;
        else
            project.Visibility = ProjectVisibility.Public;

        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }


    private IQueryable<Project> GetActiveProjects()
    {
        return _dbContext.Projects.Where(p => p.DeletedAt == null);
    }

    public async Task<Result> RemoveMemberFromProject(string projectId, string userId, string memberId, CancellationToken cancellationToken = default)
    {
        var project = await GetActiveProjects()
         .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound); 

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        var projectMember = await _dbContext.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == memberId, cancellationToken);

        if (projectMember is null)
            return Result.Failure(ProjectErrors.MemberNotFound);

          _dbContext.ProjectMembers.Remove(projectMember);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }
}
