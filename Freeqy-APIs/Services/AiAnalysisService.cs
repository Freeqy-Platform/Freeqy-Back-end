using Freeqy_APIs.Contracts.AiAnalysis;

namespace Freeqy_APIs.Services;

public class AiAnalysisService(IAiServices aiServices, ILogger<AiAnalysisService> logger): IAiAnalysisService
{
	private readonly ILogger<AiAnalysisService> _logger = logger;
	private readonly IAiServices _aiServices = aiServices;

	public async Task<TeamStructureResponse> TeamGenerator(ProjectIdea projectIdea)
	{
		var result = await _aiServices.GenerateTeamStructureAsync(projectIdea);
		return result;
	}

	public async Task<TechStackResponse> TechStackGenerator(ProjectIdea projectIdea)
	{
		var result = await _aiServices.AnalyzeTechStackAsync(projectIdea);
		return result;
	}

	public async Task<FullAnalysisResponse> FullAnalysisGenerator(ProjectIdea projectIdea)
	{
		var result = await _aiServices.GenerateFullAnalysisAsync(projectIdea);
		return result;
	}
}
