using Freeqy_APIs.Contracts.Projects;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

/// <summary>
/// Manages project invitations for team collaboration.
/// Handles sending, responding to, and managing project invitations.
/// </summary>
[ApiController]
[Route("api/[Controller]")]
[Authorize]
[EnableRateLimiting("api")]
public class ProjectInvitationsController(
    IProjectInvitationService invitationService) : ControllerBase
{
    private readonly IProjectInvitationService _invitationService = invitationService;

    /// <summary>
    /// Sends an invitation to join a specific project.
    /// </summary>
    /// <param name="projectId">The ID of the project to invite users to.</param>
    /// <param name="request">The invitation request containing user details and invitation message.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The created project invitation details.</returns>
    /// <response code="200">Invitation sent successfully.</response>
    /// <response code="400">Bad request - invalid project ID or request data.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to send invitations for this project.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpPost("{projectId}/invitations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendInvitation(string projectId, [FromBody] SendProjectInvitationRequest request, CancellationToken cancellationToken)
    {
        var result = await _invitationService.SendInvitationAsync(projectId, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all project invitations for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A list of invitations received by the user.</returns>
    /// <response code="200">Invitations retrieved successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    [HttpGet("invitations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyInvitations(CancellationToken cancellationToken)
    {
        var result = await _invitationService.GetUserInvitationsAsync(User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves detailed information about a specific project invitation.
    /// </summary>
    /// <param name="invitationId">The ID of the invitation to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The detailed information of the invitation including project and sender details.</returns>
    /// <response code="200">Invitation details retrieved successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Not found - invitation not found.</response>
    [HttpGet("invitations/{invitationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvitationDetails(string invitationId, CancellationToken cancellationToken)
    {
        var result = await _invitationService.GetInvitationDetailsAsync(invitationId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Responds to a project invitation (accept or decline).
    /// </summary>
    /// <param name="request">The response request containing the invitation ID and response type.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The updated invitation status.</returns>
    /// <response code="200">Response to invitation recorded successfully.</response>
    /// <response code="400">Bad request - invalid invitation response data.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Not found - invitation not found.</response>
    [HttpPost("invitations/respond")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RespondToInvitation([FromBody] RespondToInvitationRequest request, CancellationToken cancellationToken)
    {
        var result = await _invitationService.RespondToInvitationAsync(User.GetUserId()!, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Resends a project invitation to the recipient.
    /// </summary>
    /// <param name="invitationId">The ID of the invitation to resend.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A success message indicating the invitation was resent.</returns>
    /// <response code="200">Invitation resent successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to resend this invitation.</response>
    /// <response code="404">Not found - invitation not found.</response>
    [HttpPost("invitations/{invitationId}/resend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendInvitation(string invitationId, CancellationToken cancellationToken)
    {
        var result = await _invitationService.ResendInvitationAsync(invitationId, User.GetUserId()!, cancellationToken);

        return result.IsSuccess ? Ok(new { message = "Invitation resent" }) : result.ToProblem();
    }

    /// <summary>
    /// Cancels a project invitation.
    /// </summary>
    /// <param name="invitationId">The ID of the invitation to cancel.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A success message indicating the invitation was cancelled.</returns>
    /// <response code="200">Invitation cancelled successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to cancel this invitation.</response>
    /// <response code="404">Not found - invitation not found.</response>
    [HttpDelete("invitations/{invitationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvitation(string invitationId, CancellationToken cancellationToken)
    {
        var result = await _invitationService.CancelInvitationAsync(invitationId, User.GetUserId()!, cancellationToken);

        return result.IsSuccess ? Ok(new { message = "Invitation cancelled" }) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all pending invitations for a specific project.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A list of all invitations sent for the specified project.</returns>
    /// <response code="200">Project invitations retrieved successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have permission to view invitations for this project.</response>
    /// <response code="404">Not found - project not found.</response>
    [HttpGet("{projectId}/invitations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectInvitations(string projectId, CancellationToken cancellationToken)
    {
        var result = await _invitationService.GetProjectInvitationsAsync(User.GetUserId()!, projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
