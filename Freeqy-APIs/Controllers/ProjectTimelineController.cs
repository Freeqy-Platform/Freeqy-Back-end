using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

/// <summary>
/// Manages project timeline and history tracking for users.
/// Provides endpoints to retrieve user project history, activity timeline, and project statistics.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("api")]
public class ProjectTimelineController(IProjectHistoryService historyService) : ControllerBase
{
    private readonly IProjectHistoryService _historyService = historyService;

    /// <summary>
    /// Retrieves the current authenticated user's project timeline.
    /// Returns paginated list of project events (joined, left, role changed, completed, deleted) ordered by most recent.
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1).</param>
    /// <param name="pageSize">Number of events per page (default: 20, max recommended: 100).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated timeline events along with user project statistics.</returns>
    /// <response code="200">Timeline retrieved successfully with events and stats.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Not found - user not found.</response>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTimeline(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _historyService.GetUserTimelineAsync(User.GetUserId()!, page, pageSize, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves the project timeline for a specific user by their ID.
    /// Returns paginated list of project events ordered by most recent.
    /// </summary>
    /// <param name="userId">The ID of the user whose timeline is being retrieved.</param>
    /// <param name="page">Page number for pagination (default: 1).</param>
    /// <param name="pageSize">Number of events per page (default: 20, max recommended: 100).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated timeline events along with user project statistics.</returns>
    /// <response code="200">Timeline retrieved successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Not found - specified user not found.</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserTimeline(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _historyService.GetUserTimelineAsync(userId, page, pageSize, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves comprehensive project statistics for the current authenticated user.
    /// Includes metrics such as total projects joined, projects owned, completed projects, and completion rate.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User project statistics including join/completion metrics and dates.</returns>
    /// <response code="200">Statistics retrieved successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Not found - user not found.</response>
    [HttpGet("me/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyStats(CancellationToken ct = default)
    {
        var result = await _historyService.GetUserStatsAsync(User.GetUserId()!, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves comprehensive project statistics for a specific user by their ID.
    /// Includes metrics such as total projects joined, projects owned, completed projects, and completion rate.
    /// </summary>
    /// <param name="userId">The ID of the user whose statistics are being retrieved.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User project statistics including join/completion metrics and dates.</returns>
    /// <response code="200">Statistics retrieved successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Not found - specified user not found.</response>
    [HttpGet("{userId}/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserStats(string userId, CancellationToken ct = default)
    {
        var result = await _historyService.GetUserStatsAsync(userId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
