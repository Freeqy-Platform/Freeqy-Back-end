using Freeqy_APIs.Contracts.AiAnalysis;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;


[ApiController]
[Route("api/[controller]")]
// [Authorize]
[EnableRateLimiting("api")]
public class AiAnalysisController(IAiAnalysisService aiAnalysisService) : ControllerBase
{
	private readonly IAiAnalysisService _aiAnalysisService = aiAnalysisService;


	[HttpPost("team/generate")]
	[ProducesResponseType(typeof(TeamStructureResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GenerateTeamStructure(
		[FromBody] ProjectIdea request, 
		CancellationToken cancellationToken)
	{
		var result = await _aiAnalysisService.TeamGenerator(
			request );

		return Ok(result);
	}
	
	[HttpPost("tech/analyze")]
	[ProducesResponseType(typeof(TechStackResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> AnalyzeTechStack(
		[FromBody] ProjectIdea request, 
		CancellationToken cancellationToken)
	{
		var result = await _aiAnalysisService.TechStackGenerator(
			request);

		return Ok(result);
	}
	
	[HttpPost("full")]
	[ProducesResponseType(typeof(FullAnalysisResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GenerateFullAnalysis(
		[FromBody] ProjectIdea request, 
		CancellationToken cancellationToken)
	{
		var result = await _aiAnalysisService.FullAnalysisGenerator(
			request);

		return Ok(result);
	}
}
