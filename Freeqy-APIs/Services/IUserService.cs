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
	Task<Result<UserProfileResponse>> UpdateSkillsAsync(string userId, UpdateUserSkillsRequest skillsRequest, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> UpdateSocialLinksAsync(string userId, UpdateSocialLinksRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateEducationsAsync(string userId, UpdateEducationsRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateCertificatesAsync(string userId, UpdateCertificatesRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateUsernameAsync(string userId, UpdateUsernameRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdatePhoneNumberAsync(string userId, UpdatePhoneNumberRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateSummaryAsync(string userId, UpdateSummaryRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateAvailabilityAsync(string userId, UpdateAvailabilityRequest request, CancellationToken cancellation = default);
	Task<Result<UserProfileResponse>> UpdateEmailAsync(string userId, UpdateEmailRequest request, CancellationToken cancellation = default);
}