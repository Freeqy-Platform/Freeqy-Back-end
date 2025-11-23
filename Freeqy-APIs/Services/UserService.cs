namespace Freeqy_APIs.Services;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;

	public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
	{
		var user = await _userManager.Users
			.Where(u => u.Id == userId)
			.ProjectToType<UserProfileResponse>()
			.SingleAsync();

		return Result.Success(user);
	}

	public async Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateUserProfileRequest request)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		// Check if data has actually changed
		var hasChanges = user.FirstName != request.FirstName || user.LastName != request.LastName;

		if (!hasChanges)
		{
			// Return current profile without updating
			var currentProfile = new UserProfileResponse(
				user.Email!,
				user.UserName!,
				user.FirstName,
				user.LastName
			);

			return Result.Success(currentProfile);
		}

		// Update user properties
		user.FirstName = request.FirstName;
		user.LastName = request.LastName;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update user profile", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure<UserProfileResponse>(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		var response = new UserProfileResponse(
			user.Email!,
			user.UserName!,
			user.FirstName,
			user.LastName
		);

		return Result.Success(response);
	}
}