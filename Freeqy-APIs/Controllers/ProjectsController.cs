using Freeqy_APIs.Contracts.Category;
using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.Technology;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    private readonly IProjectService _projectService = projectService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProjectRequestFilter filter, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(filter, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(string id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddProject([FromBody] ProjectRequest projectRequest, CancellationToken cancellationToken)
    {
        var result = await _projectService.AddProjectAsync(User.GetUserId()!, projectRequest, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetProjectById), new { id = result.Value.Id }, result.Value) : result.ToProblem();
    }

    [HttpPut("{projectId}")]
    public async Task<IActionResult> UpdateProject(string projectId, [FromBody] ProjectRequest projectRequest, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateProjectAsync(projectId, User.GetUserId()!, projectRequest, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{projectId}")]
    [Authorize]
    public async Task<IActionResult> DeleteProject(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.DeleteProjectAsync(projectId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPatch("{projectId}/status")]
    [Authorize]
    public async Task<IActionResult> ChangeProjectStatus(string projectId, ChangeProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.ChangeProjectStatusAsync(User.GetUserId(), projectId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{projectId}/restore")]
    [Authorize]
    public async Task<IActionResult> RestoreProject(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.RestoreProjectAsync(projectId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPatch("{projectId}/change-visibility")]
    [Authorize]
    public async Task<IActionResult> ChangeProjectVisibility(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.ChangeProjectVisibilityAsync(projectId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("technologies")]
    public async Task<IActionResult> AddTechnology([FromBody] TechnologyRequest technologyRequest,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.AddTechnologyAsync(technologyRequest, cancellationToken);
        return result.IsSuccess ?
            CreatedAtAction(nameof(GetTechnologyById), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpGet("technologies")]
    public async Task<IActionResult> GetTechnologies(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetTechnologiesAsync(cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("technologies/{id}")]
    public async Task<IActionResult> GetTechnologyById(string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetTechnologyByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("categories")]
    public async Task<IActionResult> AddCategory([FromBody] CategoryRequest categoryRequest,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.AddCategoryAsync(categoryRequest, cancellationToken);
        return result.IsSuccess ?
            CreatedAtAction(nameof(GetCategoryById), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetCategoriesAsync(cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategoryById(string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetCategoryByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{projectId}/members/{memberId}")]
    [Authorize]
    public async Task<IActionResult> RemoveMemberFromProject(string projectId, string memberId,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.RemoveMemberFromProject(projectId, User.GetUserId()!, memberId,
            cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{projectId}/members")]
    public async Task<IActionResult> GetProjectMembers(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectMembersAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPatch("{projectId}/members/{memberId}/role")]
    [Authorize]
    public async Task<IActionResult> UpdateMemberRole(string projectId, string memberId, [FromBody] UpdateMemberRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateMemberRoleAsync(projectId, User.GetUserId()!, memberId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
