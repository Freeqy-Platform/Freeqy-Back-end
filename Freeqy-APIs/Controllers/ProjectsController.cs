using Freeqy_APIs.Contracts.Category;
using Freeqy_APIs.Contracts.Projects;
using Freeqy_APIs.Contracts.Technology;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    private readonly IProjectService _projectService = projectService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    public async Task<IActionResult> AddProject([FromBody] ProjectRequest projectRequest, CancellationToken cancellationToken)
    {
        var result = await _projectService.AddProjectAsync( User.GetUserId()!,projectRequest, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    [HttpPut("{projectId}")]
    public async Task<IActionResult> UpdateProject(string projectId, [FromBody] ProjectRequest projectRequest, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateProjectAsync(projectId,User.GetUserId()!, projectRequest, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("technologies")]
    public async Task<IActionResult> AddTechnology([FromBody] TechnologyRequest technologyRequest,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.AddTechnologyAsync(technologyRequest, cancellationToken);
        return result.IsSuccess ?
            CreatedAtAction(nameof(GetTechnologyByIdAsync), new {id = result.Value.Id}, result.Value) 
            : result.ToProblem();
    }

    [HttpGet("technologies")]
    public async Task<IActionResult> GetTechnologies(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetTechnologiesAsync(cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost("technologies/{id}")]
    public async Task<IActionResult> GetTechnologyByIdAsync(string id,
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
            CreatedAtAction(nameof(GetCategoryById), new {id = result.Value.Id}, result.Value) 
            : result.ToProblem();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetCategoriesAsync(cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost("categories/{id}")]
    public async Task<IActionResult> GetCategoryById(string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _projectService.GetCategoryByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
