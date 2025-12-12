using Freeqy_APIs.Contracts.Category;
using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.Technology;

namespace Freeqy_APIs.Services;

public interface IProjectService
{
    Task<Result<ProjectListResponse>> GetProjectsAsync(ProjectRequestFilter filter, CancellationToken cancellationToken = default);

    Task<Result<TechnologyResponse>> AddTechnologyAsync(TechnologyRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CategoryResponse>> AddCategoryAsync(CategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CategoryResponse>> GetCategoryByIdAsync(string id,
        CancellationToken cancellationToken = default);

    Task<Result<TechnologyResponse>> GetTechnologyByIdAsync(string id,
        CancellationToken cancellationToken = default);
    
    Task<Result<List<TechnologyResponse>>> GetTechnologiesAsync(
        CancellationToken cancellationToken = default);

    Task<Result<List<CategoryResponse>>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<Result<ProjectListItemResponse>> AddProjectAsync(string userId, ProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateProjectAsync(string projectId, string userId, ProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteProjectAsync(string projectId, string userId,
        CancellationToken cancellationToken = default);

    public Task<Result> RestoreProjectAsync(string projectId, string userId,
        CancellationToken cancellationToken = default);

    public Task<Result> ChangeProjectVisibilityAsync(string projectId, string userId,CancellationToken cancellationToken = default);

    public Task<Result> RemoveMemberFromProject(string projectId, string userId, string memberId,
        CancellationToken cancellationToken = default);
        
    public Task<Result<ProjectListItemResponse>> GetProjectByIdAsync(string id,CancellationToken cancellationToken = default);

    Task<Result> ChangeProjectStatusAsync(string userId, string id, ChangeProjectStatusRequest request,
        CancellationToken cancellationToken = default);
}
