namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    private readonly IProjectService _projectService = projectService;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(cancellationToken);

        if (result.IsFailure)
            return result.ToProblem();

        return Ok(result.Value);
    }
}
