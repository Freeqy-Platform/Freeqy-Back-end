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
        CancellationToken cancellationToken = default)
    {
        var projectList = await _dbContext.Projects
            .AsNoTracking()
            .ProjectToType<ProjectListItemResponse>() 
            .ToListAsync(cancellationToken);
        return Result.Success(new ProjectListResponse(projectList));
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
        
        Category category = request.Adapt<Category>();
        
        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success(category.Adapt<TechnologyResponse>());
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

    public async Task<Result<ProjectListResponse>> AddProjectAsync(string userId, ProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var isExistingProjectName = await _dbContext.Projects.AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (isExistingProjectName) return Result.Failure<ProjectListResponse>(ProjectErrors.DuplicateName);
        
        var category = await _dbContext.Categories.FindAsync(request.CategoryId, cancellationToken);
        if (category is null) 
            return Result.Failure<ProjectListResponse>(CategoryErrors.NotFound);

        var technologies = await _dbContext.Technologies
            .Where(x => request.TechnologyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (technologies.Count != request.TechnologyIds.Count)
            return Result.Failure<ProjectListResponse>(TechnologyErrors.NotFound);
        
        var user = _userManager.FindByIdAsync(userId).Result;
        
        Project project = request.Adapt<Project>();
        project.Category = category;
        project.Technologies = technologies;
        project.Owner =  user!;
        project.OwnerId  = userId; 
        
        await _dbContext.Projects.AddAsync(project, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success(project.Adapt<ProjectListResponse>());
    }

    public async Task<Result> UpdateProjectAsync(
        string projectId,
        string userId,
        ProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.Technologies)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        if (project.OwnerId != userId)
            return Result.Failure(ProjectErrors.Forbidden);

        var isDuplicateName = await _dbContext.Projects
            .AnyAsync(p => p.Name == request.Name && p.Id != projectId, cancellationToken);

        if (isDuplicateName)
            return Result.Failure(ProjectErrors.DuplicateName);

        var category = await _dbContext.Categories.FindAsync(request.CategoryId, cancellationToken);
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

        project.Technologies.Clear();
        project.Technologies = technologies;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

}
