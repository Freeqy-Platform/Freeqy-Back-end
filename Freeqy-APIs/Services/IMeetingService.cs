using Freeqy_APIs.Abstractions;
using Freeqy_APIs.Contracts.Meetings;

namespace Freeqy_APIs.Services;

public interface IMeetingService
{
    Task<Result<MeetingResponse>> CreateMeetingAsync(CreateMeetingRequest request, string userId, CancellationToken cancellationToken = default);

    Task<Result<MeetingResponse>> GetMeetingByIdAsync(string meetingId, CancellationToken cancellationToken = default);

    Task<Result<List<MeetingResponse>>> GetMeetingsByProjectIdAsync(string projectId, CancellationToken cancellationToken = default);

    Task<Result<List<MeetingResponse>>> GetUpcomingMeetingsForProjectAsync(string projectId, CancellationToken cancellationToken = default);

    Task<Result<MeetingResponse>> UpdateMeetingAsync(string meetingId, UpdateMeetingRequest request, string userId, CancellationToken cancellationToken = default);

    Task<Result> DeleteMeetingAsync(string meetingId, string userId, CancellationToken cancellationToken = default);
}
