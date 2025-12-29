namespace Freeqy_APIs.Services;

public interface IAiAnalysisService
{
   Task<TeamStructureResponse> TeamGenerator(ProjectIdea projectIdea);
   Task<TechStackResponse> TechStackGenerator(ProjectIdea projectIdea);
    Task<FullAnalysisResponse> FullAnalysisGenerator(ProjectIdea projectIdea);
}