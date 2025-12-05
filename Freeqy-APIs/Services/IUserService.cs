namespace Freeqy_APIs.Services;

public interface IUserService
{
	Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
	Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateUserProfileRequest request);
	Task<Result<UserProfileResponse>> GetUserByIdAsync(string userId, CancellationToken cancellation = default);
	Task<Result<IEnumerable<UserProfileResponse>>> GetAllAsync(UserProfileRequestFilter profileRequestFilter, CancellationToken cancellationToken = default);
	Task<Result<string>> GetUserPhotoUrlAsync(string userId);
	Task<Result<UploadPhotoResponse>> UploadUserPhotoAsync(string userId, IFormFile photo);
	Task<Result<UserProfileResponse>> UpdateSkillsAsync(string userId, UpdateUserSkillsRequest skillsRequest, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> UpdateSocialLinksAsync(string userId, UpdateSocialLinksRequest request, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> UpdateEducationsAsync(string userId, UpdateEducationsRequest request, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> UpdateCertificatesAsync(string userId, UpdateCertificatesRequest request, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> UpdateUsernameAsync(string userId, UpdateUsernameRequest request, CancellationToken cancellationToken = default);
	Task<Result<UserProfileResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
}