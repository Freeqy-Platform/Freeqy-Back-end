namespace Freeqy_APIs.BackgroundJobs;

/// <summary>
/// Background job that periodically deletes notifications older than 90 days
/// to keep the database clean.
/// </summary>
public class NotificationCleanupJob(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationCleanupJob> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<NotificationCleanupJob> _logger = logger;

    /// <summary>Run once per day at midnight.</summary>
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    /// <summary>Notifications older than this are deleted.</summary>
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(90);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Cleanup Job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "An error occurred during notification cleanup.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running notification cleanup at {Time}.", DateTime.UtcNow);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoffDate = DateTime.UtcNow - RetentionPeriod;

        var deletedCount = await dbContext.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Notification cleanup completed. Deleted {Count} old notifications.", deletedCount);
    }
}
