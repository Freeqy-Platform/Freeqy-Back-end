using Freeqy_APIs.Abstractions;
using Freeqy_APIs.Contracts.Meetings;
using Freeqy_APIs.Entities;
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

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(meeting.Adapt<MeetingResponse>());
    }

    public async Task<Result<MeetingResponse>> GetMeetingByIdAsync(string meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.DeletedAt == null, cancellationToken);

        if (meeting is null)
            return Result.Failure<MeetingResponse>(MeetingErrors.MeetingNotFound);

        return Result.Success(meeting.Adapt<MeetingResponse>());
    }

    public async Task<Result<List<MeetingResponse>>> GetMeetingsByProjectIdAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var meetings = await _context.Meetings
            .Where(m => m.ProjectId == projectId && m.DeletedAt == null)
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync(cancellationToken);

        return Result.Success(meetings.Adapt<List<MeetingResponse>>());
    }

    public async Task<Result<List<MeetingResponse>>> GetUpcomingMeetingsForProjectAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var meetings = await _context.Meetings
            .Where(m => m.ProjectId == projectId && m.DeletedAt == null && m.ScheduledAt > now)
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync(cancellationToken);

        return Result.Success(meetings.Adapt<List<MeetingResponse>>());
    }

    public async Task<Result<MeetingResponse>> UpdateMeetingAsync(string meetingId, UpdateMeetingRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.DeletedAt == null, cancellationToken);

        if (meeting is null)
            return Result.Failure<MeetingResponse>(MeetingErrors.MeetingNotFound);

        // Check authorization - only project owner or members can update
        var isAuthorized = meeting.Project.OwnerId == userId ||
            await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == meeting.ProjectId && pm.UserId == userId, cancellationToken);

        if (!isAuthorized)
            return Result.Failure<MeetingResponse>(MeetingErrors.Unauthorized);

        meeting.Title = request.Title;
        meeting.Description = request.Description;
        meeting.ScheduledAt = request.ScheduledAt;
        meeting.MeetingLink = request.MeetingLink;
        meeting.UpdatedAt = DateTime.UtcNow;

        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(meeting.Adapt<MeetingResponse>());
    }

    public async Task<Result> DeleteMeetingAsync(string meetingId, string userId, CancellationToken cancellationToken = default)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.DeletedAt == null, cancellationToken);

        if (meeting is null)
            return Result.Failure(MeetingErrors.MeetingNotFound);

        // Check authorization
        var isAuthorized = meeting.Project.OwnerId == userId ||
            await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == meeting.ProjectId && pm.UserId == userId, cancellationToken);

        if (!isAuthorized)
            return Result.Failure(MeetingErrors.Unauthorized);

        meeting.DeletedAt = DateTime.UtcNow;
        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RestoreMeetingAsync(string meetingId, string userId, CancellationToken cancellationToken = default)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.DeletedAt != null, cancellationToken);

        if (meeting is null)
            return Result.Failure(MeetingErrors.MeetingNotFound);

        // Check authorization - only project owner can restore
        if (meeting.Project.OwnerId != userId)
            return Result.Failure(MeetingErrors.Unauthorized);

        meeting.DeletedAt = null;
        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
