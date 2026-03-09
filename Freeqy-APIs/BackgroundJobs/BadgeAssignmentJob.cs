namespace Freeqy_APIs.BackgroundJobs;

public class BadgeAssignmentJob(
    IServiceScopeFactory scopeFactory,
    ILogger<BadgeAssignmentJob> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<BadgeAssignmentJob> _logger = logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Badge Assignment Job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "An error occurred during badge assignment.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running badge assignment check at {Time}.", DateTime.UtcNow);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var badgeService = scope.ServiceProvider.GetRequiredService<IBadgeService>();

        await badgeService.AssignBadgesForAllUsersAsync(cancellationToken);

        _logger.LogInformation("Badge assignment check completed.");
    }
}
