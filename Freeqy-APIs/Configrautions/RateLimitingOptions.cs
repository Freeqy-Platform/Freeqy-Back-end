namespace Freeqy_APIs.Configrautions;

public class RateLimitingOptions
{
	public const string SectionName = "RateLimiting";

	public RateLimitPolicy Global { get; set; } = new();
	public RateLimitPolicy Authentication { get; set; } = new();
	public RateLimitPolicy Api { get; set; } = new();
}

public class RateLimitPolicy
{
	public int PermitLimit { get; set; }
	public int Window { get; set; }
	public int QueueLimit { get; set; }
}
