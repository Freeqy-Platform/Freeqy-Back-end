using Freeqy_APIs.Contracts.Tracks;

namespace Freeqy_APIs.Services;

public interface IUserService
{
	Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
	Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateUserProfileRequest request);
	Task<Result<UserProfileResponse>> GetUserByIdAsync(string userId, CancellationToken cancellation = default);
	Task<Result<IEnumerable<UserProfileResponse>>> GetAllAsync(UserProfileRequestFilter profileRequestFilter, CancellationToken cancellationToken = default);
	Task<Result<string>> GetUserPhotoUrlAsync(string userId);
	Task<Result<UploadPhotoResponse>> UploadUserPhotoAsync(string userId, IFormFile photo);
	Task<Result> DeleteUserPhotoAsync(string userId);
	Task<Result<string>> GetUserBannerPhotoUrlAsync(string userId);
	Task<Result<UploadPhotoResponse>> UploadUserBannerPhotoAsync(string userId, IFormFile bannerPhoto);
	Task<Result> DeleteUserBannerPhotoAsync(string userId);
	Task<Result<UserProfileResponse>> UpdateSkillsAsync(string userId, UpdateUserSkillsRequest skillsRequest, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> UpdateSocialLinksAsync(string userId, UpdateSocialLinksRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateEducationsAsync(string userId, UpdateEducationsRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateCertificatesAsync(string userId, UpdateCertificatesRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateUsernameAsync(string userId, UpdateUsernameRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdatePhoneNumberAsync(string userId, UpdatePhoneNumberRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateSummaryAsync(string userId, UpdateSummaryRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateAvailabilityAsync(string userId, UpdateAvailabilityRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateTrackAsync(string userId, UpdateTrackRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateTrackWithConfirmAsync(string userId, UpdateTrackWithConfirmRequest request, CancellationToken cancellation = default);
	Task<Result<List<TrackResponse>>> GetTracksAsync(CancellationToken cancellationToken = default);
	
	// Track Request System
	Task<Result<TrackRequestResponse>> CreateTrackRequestAsync(string userId, CreateTrackRequestDto request, CancellationToken cancellationToken = default);
	Task<Result<TrackRequestListResponse>> GetUserTrackRequestsAsync(string userId, CancellationToken cancellationToken = default);
	Task<Result<TrackRequestListResponse>> GetAllTrackRequestsAsync(TrackRequestStatus? status, CancellationToken cancellationToken = default);
	Task<Result<UserTrackRequestStatsResponse>> GetUserTrackRequestStatsAsync(string userId, CancellationToken cancellationToken = default);
	Task<Result> ApproveTrackRequestAsync(string adminId, ApproveTrackRequestDto request, CancellationToken cancellationToken = default);
	Task<Result> RejectTrackRequestAsync(string adminId, RejectTrackRequestDto request, CancellationToken cancellationToken = default);
	
	Task<Result<UserProfileResponse>> UpdateEmailAsync(string userId, UpdateEmailRequest request, CancellationToken cancellation = default);
	Task<Result> UpdatePasswordAsync(string userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default);
	Task<Result> ConfirmEmailChangeAsync(string userId, string token, CancellationToken cancellationToken = default);
}