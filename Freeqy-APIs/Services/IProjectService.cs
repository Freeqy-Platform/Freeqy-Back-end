using Freeqy_APIs.Contracts.Category;
using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.Technology;

namespace Freeqy_APIs.Services;

public interface IProjectService
{
    Task<Result<ProjectListResponse>> GetProjectsAsync(CancellationToken cancellationToken = default);

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

    Task<Result<ProjectListResponse>> AddProjectAsync(string userId, ProjectRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateProjectAsync(string projectId, string userId, ProjectRequest request,
        CancellationToken cancellationToken = default);
}
