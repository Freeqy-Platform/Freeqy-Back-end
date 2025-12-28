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
					Role: "Project Manager",
					Track: "Product Development",
					RecommendedMembers: 1,
					Skills: ["Agile Methodologies", "Project Planning", "Team Management"],
					Priority: "High"
				),
				new RoleRecommendation(
					Role: "AI/ML Engineer",
					Track: "Engineering",
					RecommendedMembers: 2,
					Skills: ["Python", "TensorFlow", "Computer Vision"],
					Priority: "High"
				),
				new RoleRecommendation(
					Role: "Mobile Developer",
					Track: "Engineering",
					RecommendedMembers: 2,
					Skills: ["React Native", "iOS", "Android"],
					Priority: "High"
				),
				new RoleRecommendation(
					Role: "Backend Developer",
					Track: "Engineering",
					RecommendedMembers: 1,
					Skills: ["Python", "FastAPI", "PostgreSQL"],
					Priority: "Medium"
				),
				new RoleRecommendation(
					Role: "UI/UX Designer",
					Track: "Design",
					RecommendedMembers: 1,
					Skills: ["Figma", "User Research", "Prototyping"],
					Priority: "Medium"
				)
			],
			TotalMembers: 7,
			RequiredSkills: ["Python", "TensorFlow", "React Native", "FastAPI", "PostgreSQL", "Figma"]
		);

		return Task.FromResult(Result.Success(response));
	}

	public Task<Result<TechStackResponse>> AnalyzeTechStackAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Analyzing tech stack for project idea (MOCK DATA)");

		var response = new TechStackResponse(
			Backend: ["Python with FastAPI", "Flask", "Node.js with Express"],
			Frontend: ["React Native", "TypeScript", "React"],
			Database: ["PostgreSQL", "Redis", "MongoDB"],
			AiStack: ["TensorFlow", "PyTorch", "OpenCV", "OpenAI API"],
			DevOps: ["Docker", "AWS", "GitHub Actions", "Kubernetes"]
		);

		return Task.FromResult(Result.Success(response));
	}

	public async Task<Result<FullAnalysisResponse>> GenerateFullAnalysisAsync(
		string projectIdea, 
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Generating full analysis for project idea (MOCK DATA)");

		var startTime = DateTime.UtcNow;
		
		var teamResult = await GenerateTeamStructureAsync(projectIdea, cancellationToken);
		var techResult = await AnalyzeTechStackAsync(projectIdea, cancellationToken);

		if (teamResult.IsFailure || techResult.IsFailure)
		{
			return Result.Failure<FullAnalysisResponse>(
				new Error("AiAnalysis.GenerationFailed", 
				         "Failed to generate complete analysis", 
				         StatusCodes.Status500InternalServerError));
		}

		var processingTime = (DateTime.UtcNow - startTime).TotalSeconds;
		
		var totalRoles = teamResult.Value.Roles.Count;
		var totalMembers = teamResult.Value.Roles.Sum(r => r.RecommendedMembers);

		var response = new FullAnalysisResponse(
			Success: true,
			TeamStructure: teamResult.Value,
			TechStack: techResult.Value,
			TotalRoles: totalRoles,
			TotalMembers: totalMembers,
			ProcessingTime: Math.Round(processingTime, 2)
		);

		return Result.Success(response);
	}
}
