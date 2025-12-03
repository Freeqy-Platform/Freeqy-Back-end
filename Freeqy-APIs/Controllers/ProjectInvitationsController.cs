using Freeqy_APIs.Contracts.Projects;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
[EnableRateLimiting("api")]
public class ProjectInvitationsController(
    IProjectInvitationService invitationService) : ControllerBase
{
    private readonly IProjectInvitationService _invitationService = invitationService;

    [HttpPost("{projectId}/invitations")]
    public async Task<IActionResult> SendInvitation(string projectId,[FromBody] SendProjectInvitationRequest request,CancellationToken cancellationToken)
    {
        var result = await _invitationService.SendInvitationAsync( projectId, User.GetUserId()!,request,cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("invitations")]
    public async Task<IActionResult> GetMyInvitations(CancellationToken cancellationToken)
    {
        var result = await _invitationService.GetUserInvitationsAsync(User.GetUserId()!,cancellationToken); 
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("invitations/{invitationId}")]
    public async Task<IActionResult> GetInvitationDetails(string invitationId,CancellationToken cancellationToken)
    {
        var result = await _invitationService.GetInvitationDetailsAsync( invitationId,cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("invitations/respond")]
    public async Task<IActionResult> RespondToInvitation([FromBody] RespondToInvitationRequest request,CancellationToken cancellationToken)         
    {
        var result = await _invitationService.RespondToInvitationAsync(User.GetUserId()!,request,cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("invitations/{invitationId}/resend")]
    public async Task<IActionResult> ResendInvitation(string invitationId,CancellationToken cancellationToken)       
    {
        var result = await _invitationService.ResendInvitationAsync( invitationId, User.GetUserId()!,cancellationToken);

        return result.IsSuccess ? Ok(new { message = "Invitation resent" }) : result.ToProblem();
    }

    [HttpDelete("invitations/{invitationId}")]
    public async Task<IActionResult> CancelInvitation(string invitationId,CancellationToken cancellationToken)  
    {
        var result = await _invitationService.CancelInvitationAsync(invitationId,User.GetUserId()!,cancellationToken);

        return result.IsSuccess ? Ok(new { message = "Invitation cancelled" }) : result.ToProblem();
    }
}
