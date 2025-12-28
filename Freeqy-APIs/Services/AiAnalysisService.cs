using Freeqy_APIs.Contracts.AiAnalysis;

namespace Freeqy_APIs.Services;

public class AiAnalysisService : IAiAnalysisService
{
	private readonly ILogger<AiAnalysisService> _logger;

	public AiAnalysisService(ILogger<AiAnalysisService> logger)
	{
		_logger = logger;
	}

	public Task<Result<TeamStructureResponse>> GenerateTeamStructureAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Generating team structure for project idea (MOCK DATA)");

		var response = new TeamStructureResponse(
			Roles:
			[
				new RoleRecommendation(
					RoleName: "Full Stack Developer",
					Track: "Full Stack Development",
					Count: 2,
					RequiredSkills: ["React", "Node.js", "MongoDB", "TypeScript"],
					Priority: "High",
					Description: "Responsible for building both frontend and backend features"
				),
				new RoleRecommendation(
					RoleName: "UI/UX Designer",
					Track: "Design",
					Count: 1,
					RequiredSkills: ["Figma", "Adobe XD", "User Research", "Prototyping"],
					Priority: "High",
					Description: "Design user interfaces and ensure great user experience"
				),
				new RoleRecommendation(
					RoleName: "Backend Developer",
					Track: "Backend Development",
					Count: 1,
					RequiredSkills: ["ASP.NET Core", "SQL Server", "RESTful APIs", "Authentication"],
					Priority: "Medium",
					Description: "Build and maintain server-side logic and APIs"
				),
				new RoleRecommendation(
					RoleName: "DevOps Engineer",
					Track: "DevOps",
					Count: 1,
					RequiredSkills: ["Docker", "CI/CD", "Azure", "Kubernetes"],
					Priority: "Medium",
					Description: "Manage deployment pipelines and infrastructure"
				)
			],
			TotalMembers: 5,
			RequiredSkills: ["React", "Node.js", "ASP.NET Core", "MongoDB", "SQL Server", "Docker", "Figma"],
			ProjectComplexity: "Medium",
			EstimatedDuration: "3-4 months"
		);

		return Task.FromResult(Result.Success(response));
	}

	public Task<Result<TechStackResponse>> AnalyzeTechStackAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Analyzing tech stack for project idea (MOCK DATA)");

		var response = new TechStackResponse(
			Backend:
			[
				new TechnologyRecommendation(
					Name: "ASP.NET Core",
					Category: "Backend Framework",
					Reason: "Robust, scalable, and great for enterprise applications",
					Priority: "High"
				),
				new TechnologyRecommendation(
					Name: "Entity Framework Core",
					Category: "ORM",
					Reason: "Simplifies database operations with strong typing",
					Priority: "High"
				)
			],
			Frontend:
			[
				new TechnologyRecommendation(
					Name: "React",
					Category: "Frontend Framework",
					Reason: "Popular, component-based, large ecosystem",
					Priority: "High"
				),
				new TechnologyRecommendation(
					Name: "Tailwind CSS",
					Category: "CSS Framework",
					Reason: "Utility-first CSS for rapid UI development",
					Priority: "Medium"
				)
			],
			Database:
			[
				new TechnologyRecommendation(
					Name: "SQL Server",
					Category: "Relational Database",
					Reason: "Enterprise-grade, reliable, integrates well with .NET",
					Priority: "High"
				),
				new TechnologyRecommendation(
					Name: "Redis",
					Category: "Cache",
					Reason: "Fast in-memory caching for performance optimization",
					Priority: "Medium"
				)
			],
			DevOps:
			[
				new TechnologyRecommendation(
					Name: "Docker",
					Category: "Containerization",
					Reason: "Consistent deployment across environments",
					Priority: "High"
				),
				new TechnologyRecommendation(
					Name: "GitHub Actions",
					Category: "CI/CD",
					Reason: "Automated testing and deployment pipelines",
					Priority: "Medium"
				)
			],
			AiStack:
			[
				new TechnologyRecommendation(
					Name: "OpenAI API",
					Category: "AI/ML",
					Reason: "Powerful NLP capabilities for intelligent features",
					Priority: "Low"
				)
			],
			ArchitecturePattern: "Clean Architecture with CQRS"
		);

		return Task.FromResult(Result.Success(response));
	}

	public async Task<Result<FullAnalysisResponse>> GenerateFullAnalysisAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Generating full analysis for project idea (MOCK DATA)");

		var teamResult = await GenerateTeamStructureAsync(projectIdea, cancellationToken);
		var techResult = await AnalyzeTechStackAsync(projectIdea, cancellationToken);

		if (teamResult.IsFailure || techResult.IsFailure)
		{
			return Result.Failure<FullAnalysisResponse>(
				new Error("AiAnalysis.GenerationFailed", 
				         "Failed to generate complete analysis", 
				         StatusCodes.Status500InternalServerError));
		}

		var response = new FullAnalysisResponse(
			TeamStructure: teamResult.Value,
			TechStack: techResult.Value,
			ProjectIdea: projectIdea,
			AnalyzedAt: DateTime.UtcNow,
			Metrics: new AnalysisMetrics(
				TeamSize: teamResult.Value.TotalMembers,
				TechnologiesCount: techResult.Value.Backend.Count + 
				                   techResult.Value.Frontend.Count + 
				                   techResult.Value.Database.Count + 
				                   techResult.Value.DevOps.Count + 
				                   techResult.Value.AiStack.Count,
				ProjectComplexity: teamResult.Value.ProjectComplexity,
				EstimatedDuration: teamResult.Value.EstimatedDuration
			)
		);

		return Result.Success(response);
	}
}
