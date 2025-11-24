namespace Freeqy_APIs.Services;

public interface IUserService
{
	Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
	Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateUserProfileRequest request);
	Task<Result<UserProfileResponse>> GetUserByIdAsync(string userId);
	Task<Result<IEnumerable<UserProfileResponse>>> GetAllAsync(UserProfileRequestFilter profileRequestFilter, CancellationToken cancellationToken);
	Task<Result<string>> GetUserPhotoUrlAsync(string userId);
	Task<Result<UploadPhotoResponse>> UploadUserPhotoAsync(string userId, IFormFile photo);
}