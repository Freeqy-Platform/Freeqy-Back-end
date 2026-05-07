using Freeqy_APIs.Contracts.Meetings;
using Freeqy_APIs.Extensions;
using Freeqy_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class MeetingsController(IMeetingService meetingService) : ControllerBase
{
    private readonly IMeetingService _meetingService = meetingService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request, CancellationToken cancellationToken)
    {
        var result = await _meetingService.CreateMeetingAsync(request, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetMeetingById), new { id = result.Value.Id }, result.Value) : result.ToProblem();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeetingById(string id, CancellationToken cancellationToken)
    {
        var result = await _meetingService.GetMeetingByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetMeetingsByProject(string projectId, CancellationToken cancellationToken)
    {
        var result = await _meetingService.GetMeetingsByProjectIdAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("project/{projectId}/upcoming")]
    public async Task<IActionResult> GetUpcomingMeetings(string projectId, CancellationToken cancellationToken)
    {
        var result = await _meetingService.GetUpcomingMeetingsForProjectAsync(projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateMeeting(string id, [FromBody] UpdateMeetingRequest request, CancellationToken cancellationToken)
    {
        var result = await _meetingService.UpdateMeetingAsync(id, request, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteMeeting(string id, CancellationToken cancellationToken)
    {
        var result = await _meetingService.DeleteMeetingAsync(id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
