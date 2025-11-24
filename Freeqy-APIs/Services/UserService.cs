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
		if (await _userManager.FindByIdAsync(userId) is not { } user)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var hasChanges = user.FirstName != request.FirstName || user.LastName != request.LastName;

		if (!hasChanges)
		{
			var profile = user.Adapt<UserProfileResponse>();

			return Result.Success(profile);
		}

		user.FirstName = request.FirstName;
		user.LastName = request.LastName;

		var result = await _userManager.UpdateAsync(user);

		if (result.Succeeded)
		{
			var response = user.Adapt<UserProfileResponse>();

			return Result.Success(response);
		}

		var error = result.Errors.FirstOrDefault();

		if (error is null)
		{
			return Result.Failure<UserProfileResponse>(
				new Error("User.UpdateFailed", "Failed to update user profile", StatusCodes.Status500InternalServerError));
		}

		return Result.Failure<UserProfileResponse>(
			new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result<UserProfileResponse>> GetUserByIdAsync(string userId)
	{
		var user = await _userManager.Users
			.Where(u => u.Id == userId)
			.ProjectToType<UserProfileResponse>()
			.SingleOrDefaultAsync();

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		return Result.Success(user);
	}

	public async Task<Result<IEnumerable<UserProfileResponse>>> GetAllAsync(UserProfileRequestFilter profileRequestFilter, CancellationToken cancellationToken)
	{
		var query = _userManager.Users.AsQueryable();

		if (!string.IsNullOrEmpty(profileRequestFilter.Name))
		{
			query = query.Where(u => u.FirstName.Contains(profileRequestFilter.Name));
		}
		if (!string.IsNullOrEmpty(profileRequestFilter.Track))
		{
			query = query.Where(u => u.Track!.Name.Contains(profileRequestFilter.Track));
		}
		if (profileRequestFilter.Skills is not null && profileRequestFilter.Skills.Any())
		{
			query = query.Where(u => u.Skills.Any(s => profileRequestFilter.Skills.Contains(s.Skill!.Name)));
		}

		var users = await query
			.Include(u => u.Track)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.ProjectToType<UserProfileResponse>()
			.ToListAsync(cancellationToken);

		return Result.Success<IEnumerable<UserProfileResponse>>(users);
	}
}