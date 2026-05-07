using Freeqy_APIs.Abstractions;
using Freeqy_APIs.Contracts.Meetings;
using Freeqy_APIs.Entities;
using Freeqy_APIs.Enums;
using Freeqy_APIs.Errors;
using Freeqy_APIs.Persistancec;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Freeqy_APIs.Services;

public class MeetingService(ApplicationDbContext context) : IMeetingService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<MeetingResponse>> CreateMeetingAsync(CreateMeetingRequest request, string userId, CancellationToken cancellationToken = default)
    {
        // Validate meeting type
        if (!Enum.IsDefined(typeof(MeetingType), request.Type))
            return Result.Failure<MeetingResponse>(MeetingErrors.InvalidMeetingType);

        // Validate location for offline meetings
        if (request.Type == MeetingType.Offline && string.IsNullOrWhiteSpace(request.Location))
            return Result.Failure<MeetingResponse>(MeetingErrors.LocationRequiredForOfflineMeeting);

        // Verify project exists and user has permission
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
            return Result.Failure<MeetingResponse>(MeetingErrors.ProjectNotFound);

        // Check if user is project owner or member
        var isAuthorized = project.OwnerId == userId || 
            await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == userId, cancellationToken);

        if (!isAuthorized)
            return Result.Failure<MeetingResponse>(MeetingErrors.Unauthorized);

        var meeting = request.Adapt<Meeting>();
        meeting.ProjectId = request.ProjectId;
        meeting.CreatedByUserId = userId;

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(meeting.Adapt<MeetingResponse>());
    }

    public async Task<Result<MeetingResponse>> GetMeetingByIdAsync(string meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure<MeetingResponse>(MeetingErrors.MeetingNotFound);

        return Result.Success(meeting.Adapt<MeetingResponse>());
    }

    public async Task<Result<List<MeetingResponse>>> GetMeetingsByProjectIdAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var meetings = await _context.Meetings
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync(cancellationToken);

        return Result.Success(meetings.Adapt<List<MeetingResponse>>());
    }

    public async Task<Result<List<MeetingResponse>>> GetUpcomingMeetingsForProjectAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var meetings = await _context.Meetings
            .Where(m => m.ProjectId == projectId && m.ScheduledAt > now)
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync(cancellationToken);

        return Result.Success(meetings.Adapt<List<MeetingResponse>>());
    }

    public async Task<Result<MeetingResponse>> UpdateMeetingAsync(string meetingId, UpdateMeetingRequest request, string userId, CancellationToken cancellationToken = default)
    {
        // Validate meeting type
        if (!Enum.IsDefined(typeof(MeetingType), request.Type))
            return Result.Failure<MeetingResponse>(MeetingErrors.InvalidMeetingType);

        // Validate location for offline meetings
        if (request.Type == MeetingType.Offline && string.IsNullOrWhiteSpace(request.Location))
            return Result.Failure<MeetingResponse>(MeetingErrors.LocationRequiredForOfflineMeeting);

        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure<MeetingResponse>(MeetingErrors.MeetingNotFound);

        // Check authorization - only the user who created the meeting can update it
        if (meeting.CreatedByUserId != userId)
            return Result.Failure<MeetingResponse>(MeetingErrors.CannotUpdateMeetingNotCreatedByUser);

        meeting.Title = request.Title;
        meeting.Description = request.Description;
        meeting.ScheduledAt = request.ScheduledAt;
        meeting.Type = request.Type;
        meeting.MeetingLink = request.MeetingLink;
        meeting.Location = request.Location;
        meeting.UpdatedAt = DateTime.UtcNow;

        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(meeting.Adapt<MeetingResponse>());
    }

    public async Task<Result> DeleteMeetingAsync(string meetingId, string userId, CancellationToken cancellationToken = default)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId, cancellationToken);

        if (meeting is null)
            return Result.Failure(MeetingErrors.MeetingNotFound);

        // Check authorization - only the user who created the meeting can delete it
        if (meeting.CreatedByUserId != userId)
            return Result.Failure(MeetingErrors.CannotDeleteMeetingNotCreatedByUser);

        _context.Meetings.Remove(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

