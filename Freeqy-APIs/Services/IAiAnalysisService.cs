using Freeqy_APIs.Contracts.AiAnalysis;

namespace Freeqy_APIs.Services;

public interface IAiAnalysisService
{
	Task<Result<TeamStructureResponse>> GenerateTeamStructureAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default);
	
	Task<Result<TechStackResponse>> AnalyzeTechStackAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default);
	
	Task<Result<FullAnalysisResponse>> GenerateFullAnalysisAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default);
}
