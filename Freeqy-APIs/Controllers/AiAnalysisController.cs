using Freeqy_APIs.Contracts.AiAnalysis;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

/// <summary>
/// Provides AI-powered project analysis including team structure and technology recommendations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("api")]
public class AiAnalysisController(IAiAnalysisService aiAnalysisService) : ControllerBase
{
	private readonly IAiAnalysisService _aiAnalysisService = aiAnalysisService;

	/// <summary>
	/// Generates team structure recommendations based on project idea.
	/// </summary>
	/// <param name="request">The project idea description.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>Recommended team structure including roles, skills, and team size.</returns>
	/// <response code="200">Team structure generated successfully.</response>
	/// <response code="400">Bad request - invalid project idea.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPost("team/generate")]
	[ProducesResponseType(typeof(TeamStructureResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GenerateTeamStructure(
		[FromBody] AiAnalysisRequest request, 
		CancellationToken cancellationToken)
	{
		var result = await _aiAnalysisService.GenerateTeamStructureAsync(
			request.ProjectIdea, 
			cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Analyzes and recommends technology stack for the project.
	/// </summary>
	/// <param name="request">The project idea description.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>Recommended technologies for backend, frontend, database, DevOps, and AI.</returns>
	/// <response code="200">Tech stack analyzed successfully.</response>
	/// <response code="400">Bad request - invalid project idea.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPost("tech/analyze")]
	[ProducesResponseType(typeof(TechStackResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> AnalyzeTechStack(
		[FromBody] AiAnalysisRequest request, 
		CancellationToken cancellationToken)
	{
		var result = await _aiAnalysisService.AnalyzeTechStackAsync(
			request.ProjectIdea, 
			cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Performs complete analysis including both team structure and tech stack recommendations.
	/// </summary>
	/// <param name="request">The project idea description.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>Complete analysis with team structure, tech stack, and project metrics.</returns>
	/// <response code="200">Full analysis generated successfully.</response>
	/// <response code="400">Bad request - invalid project idea.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPost("full")]
	[ProducesResponseType(typeof(FullAnalysisResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GenerateFullAnalysis(
		[FromBody] AiAnalysisRequest request, 
		CancellationToken cancellationToken)
	{
		var result = await _aiAnalysisService.GenerateFullAnalysisAsync(
			request.ProjectIdea, 
			cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}
}
