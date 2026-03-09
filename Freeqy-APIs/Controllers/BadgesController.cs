using Freeqy_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

/// <summary>
/// Manages badges and achievements earned by users.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("api")]
public class BadgesController(IBadgeService badgeService) : ControllerBase
{
    private readonly IBadgeService _badgeService = badgeService;

    /// <summary>
    /// Retrieves all badges earned by the current authenticated user.
    /// </summary>
    /// <returns>A list of the user's earned badges.</returns>
    /// <response code="200">Badges retrieved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyBadges(CancellationToken cancellationToken)
    {
        var result = await _badgeService.GetUserBadgesAsync(User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all badges earned by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the target user.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of the user's earned badges.</returns>
    /// <response code="200">Badges retrieved successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserBadges(string userId, CancellationToken cancellationToken)
    {
        var result = await _badgeService.GetUserBadgesAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Manually triggers badge evaluation for the current user.
    /// Useful after completing a significant action.
    /// </summary>
    /// <response code="204">Badge evaluation completed.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("me/evaluate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EvaluateMyBadges(CancellationToken cancellationToken)
    {
        await _badgeService.AssignBadgesAsync(User.GetUserId()!, cancellationToken);
        return NoContent();
    }
}
