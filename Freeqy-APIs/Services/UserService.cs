namespace Freeqy_APIs.Services;

public class UserService(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment) : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IWebHostEnvironment _environment = environment;
	private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
	private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

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

	public async Task<Result<string>> GetUserPhotoUrlAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<string>(UserErrors.UserNotFound);

		if (string.IsNullOrEmpty(user.PhotoUrl))
			return Result.Failure<string>(UserErrors.PhotoNotFound);

		return Result.Success(user.PhotoUrl);
	}

	public async Task<Result<UploadPhotoResponse>> UploadUserPhotoAsync(string userId, IFormFile photo)
	{
		// 1. Validate photo file
		if (photo is null || photo.Length == 0)
			return Result.Failure<UploadPhotoResponse>(UserErrors.NoPhotoProvided);

		// 2. Check file size
		if (photo.Length > MaxFileSize)
			return Result.Failure<UploadPhotoResponse>(UserErrors.PhotoFileTooLarge);

		// 3. Check file extension
		var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
		if (!AllowedExtensions.Contains(extension))
			return Result.Failure<UploadPhotoResponse>(UserErrors.InvalidPhotoFile);

		// 4. Get user
		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
			return Result.Failure<UploadPhotoResponse>(UserErrors.UserNotFound);

		// 5. Delete old photo if exists
		if (!string.IsNullOrEmpty(user.PhotoUrl))
		{
			var oldPhotoPath = Path.Combine(_environment.WebRootPath, user.PhotoUrl.TrimStart('/'));
			if (File.Exists(oldPhotoPath))
			{
				File.Delete(oldPhotoPath);
			}
		}

		// 6. Generate unique filename using GUID
		var fileName = $"{Guid.CreateVersion7()}{extension}";
		var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-photos");
		var filePath = Path.Combine(uploadsFolder, fileName);

		// 7. Ensure directory exists
		Directory.CreateDirectory(uploadsFolder);

		// 8. Save file to disk
		await using (var fileStream = new FileStream(filePath, FileMode.Create))
		{
			await photo.CopyToAsync(fileStream);
		}

		// 9. Update user's PhotoUrl
		var photoUrl = $"/uploads/profile-photos/{fileName}";
		user.PhotoUrl = photoUrl;

		var updateResult = await _userManager.UpdateAsync(user);

		if (!updateResult.Succeeded)
		{
			// Rollback: delete the uploaded file
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			var error = updateResult.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UploadPhotoResponse>(
					new Error("User.UpdateFailed", "Failed to update user profile", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure<UploadPhotoResponse>(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		// 10. Return success response
		var response = new UploadPhotoResponse(photoUrl, "Profile photo uploaded successfully");
		return Result.Success(response);
	}
}