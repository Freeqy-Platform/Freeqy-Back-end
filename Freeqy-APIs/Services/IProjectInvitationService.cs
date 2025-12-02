using Freeqy_APIs.Contracts.Projects;

namespace Freeqy_APIs.Services;

public interface IProjectInvitationService
{
    Task<Result<ProjectInvitationResponse>> SendInvitationAsync(string projectId,string senderId,SendProjectInvitationRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<UserInvitationsResponse>> GetUserInvitationsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Result<InvitationDetailResponse>> GetInvitationDetailsAsync(
        string invitationId,
        CancellationToken cancellationToken = default);

    Task<Result<RespondToInvitationResponse>> RespondToInvitationAsync(string userId,RespondToInvitationRequest request,
        CancellationToken cancellationToken = default);
        
    Task<Result> ResendInvitationAsync(string invitationId, string userId, CancellationToken cancellationToken = default);
    Task<Result> CancelInvitationAsync(string invitationId, string userId,CancellationToken cancellationToken = default);
 
}
