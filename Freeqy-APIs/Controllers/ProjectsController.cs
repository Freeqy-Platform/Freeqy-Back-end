using Freeqy_APIs.Contracts.Category;
using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.Technology;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

/// <summary>
/// Manages projects, including creation, retrieval, updating, and deletion.
/// Also handles project-related resources such as technologies, categories, and project members.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    private readonly IProjectService _projectService = projectService;

    /// <summary>
    /// Retrieves all projects with optional filtering.
    /// </summary>
    /// <param name="filter">The filter criteria for projects (search, category, status, etc.).</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A paginated list of projects matching the filter criteria.</returns>
    /// <response code="200">Projects retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ProjectRequestFilter filter, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(filter, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves a specific project by its ID.
    /// </summary>
    /// <param name="id">The ID of the project to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The project details including members, technologies, and metadata.</returns>
    /// <response code="200">Project retrieved successfully.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(string id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    
    /// <summary>
    /// Creates a new project for the authenticated user.
    /// </summary>
    /// <param name="projectRequest">The project details including name, description, category, and technologies.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The newly created project with its ID and metadata.</returns>
    /// <response code="201">Project created successfully.</response>
    /// <response code="400">Bad request - invalid project data.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddProject([FromBody] ProjectRequest projectRequest, CancellationToken cancellationToken)
    {
        var result = await _projectService.AddProjectAsync( User.GetUserId()!,projectRequest, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetProjectById), new {id = result.Value.Id}, result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="projectId">The ID of the project to update.</param>
    /// <param name="projectRequest">The updated project details.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <response code="204">Project updated successfully.</response>
    /// <response code="400">Bad request - invalid project data.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to update this project.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpPut("{projectId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(string projectId, [FromBody] ProjectRequest projectRequest, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateProjectAsync(projectId,User.GetUserId()!, projectRequest, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>
    /// Deletes a project permanently.
    /// </summary>
    /// <param name="projectId">The ID of the project to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <response code="204">Project deleted successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to delete this project.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpDelete("{projectId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.DeleteProjectAsync(projectId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>
    /// Changes the status of a project (e.g., active, archived, completed).
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="request">The new status for the project.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <response code="204">Project status changed successfully.</response>
    /// <response code="400">Bad request - invalid status value.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to change project status.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpPatch("{projectId}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeProjectStatus(string projectId, ChangeProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.ChangeProjectStatusAsync(User.GetUserId(),  projectId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
    
    /// <summary>
    /// Restores a deleted or archived project.
    /// </summary>
    /// <param name="projectId">The ID of the project to restore.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <response code="200">Project restored successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to restore this project.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpPost("{projectId}/restore")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProject(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.RestoreProjectAsync(projectId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    /// <summary>
    /// Changes the visibility of a project (public/private).
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <response code="200">Project visibility changed successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to change project visibility.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpPatch("{projectId}/change-visibility")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeProjectVisibility(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.ChangeProjectVisibilityAsync(projectId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    /// <summary>
    /// Creates a new technology that can be used in projects.
    /// </summary>
    /// <param name="technologyRequest">The technology details including name and description.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The newly created technology with its ID.</returns>
    /// <response code="201">Technology created successfully.</response>
    /// <response code="400">Bad request - invalid technology data.</response>
    [HttpPost("technologies")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTechnology([FromBody] TechnologyRequest technologyRequest,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.AddTechnologyAsync(technologyRequest, cancellationToken);
        return result.IsSuccess ?
            CreatedAtAction(nameof(GetTechnologyById), new {id = result.Value.Id}, result.Value) 
            : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all available technologies.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A list of all technologies in the system.</returns>
    /// <response code="200">Technologies retrieved successfully.</response>
    [HttpGet("technologies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTechnologies(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetTechnologiesAsync(cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves a specific technology by its ID.
    /// </summary>
    /// <param name="id">The ID of the technology to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The technology details.</returns>
    /// <response code="200">Technology retrieved successfully.</response>
    /// <response code="404">Not found - technology not found.</response>
    [HttpGet("technologies/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTechnologyById(string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetTechnologyByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    
    /// <summary>
    /// Creates a new project category.
    /// </summary>
    /// <param name="categoryRequest">The category details including name and description.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The newly created category with its ID.</returns>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Bad request - invalid category data.</response>
    [HttpPost("categories")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCategory([FromBody] CategoryRequest categoryRequest,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.AddCategoryAsync(categoryRequest, cancellationToken);
        return result.IsSuccess ?
            CreatedAtAction(nameof(GetCategoryById), new {id = result.Value.Id}, result.Value) 
            : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all available project categories.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A list of all project categories.</returns>
    /// <response code="200">Categories retrieved successfully.</response>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetCategoriesAsync(cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves a specific category by its ID.
    /// </summary>
    /// <param name="id">The ID of the category to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The category details.</returns>
    /// <response code="200">Category retrieved successfully.</response>
    /// <response code="404">Not found - category not found.</response>
    [HttpGet("categories/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetCategoryByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Removes a member from a project.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="memberId">The ID of the member to remove.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <response code="204">Member removed successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to remove members.</response>
    /// <response code="404">Not found - project or member not found.</response>
    [HttpDelete("{projectId}/members/{memberId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMemberFromProject(string projectId, string memberId,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.RemoveMemberFromProject(projectId, User.GetUserId()!, memberId,
            cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all members of a specific project.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A list of project members with their roles and details.</returns>
    /// <response code="200">Project members retrieved successfully.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpGet("{projectId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectMembers(string projectId, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectMembersAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Updates the role of a project member.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="memberId">The ID of the member whose role will be updated.</param>
    /// <param name="request">The new role for the member.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <response code="204">Member role updated successfully.</response>
    /// <response code="400">Bad request - invalid role value.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to update member roles.</response>
    /// <response code="404">Not found - project or member not found.</response>
    [HttpPatch("{projectId}/members/{memberId}/role")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemberRole(string projectId, string memberId, [FromBody] UpdateMemberRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateMemberRoleAsync(projectId, User.GetUserId()!, memberId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
