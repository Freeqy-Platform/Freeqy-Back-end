using Freeqy_APIs.Contracts.Projects;

namespace Freeqy_APIs.Services;

public interface IProjectService
{
    Task<Result<ProjectListResponse>> GetProjectsAsync(CancellationToken cancellationToken = default);
}
