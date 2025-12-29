using Freeqy_APIs.Contracts.AiAnalysis;
using Refit;

namespace Freeqy_APIs.Services;

public interface IAiServices
{
	
	[Post("/team/generate")]
	Task<TeamStructureResponse> GenerateTeamStructureAsync(
		[Body] ProjectIdea idea);
	
	[Post("/tech/analyze")]
	Task<TechStackResponse> AnalyzeTechStackAsync(
		[Body] ProjectIdea idea);
	
	[Post("/analysis/full")]
	Task<FullAnalysisResponse> GenerateFullAnalysisAsync(
		[Body] ProjectIdea idea);
}
