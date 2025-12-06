using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Freeqy_APIs.Services;

public class UserService(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, ApplicationDbContext context) : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IWebHostEnvironment _environment = environment;
	private readonly ApplicationDbContext _context = context;
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

	public async Task<Result<UserProfileResponse>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Where(u => u.Id == userId)
			.ProjectToType<UserProfileResponse>()
			.SingleOrDefaultAsync(cancellationToken);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		return Result.Success(user);
	}

	public async Task<Result<IEnumerable<UserProfileResponse>>> GetAllAsync(UserProfileRequestFilter profileRequestFilter, CancellationToken cancellationToken = default)
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

	public async Task<Result<UserProfileResponse>> UpdateSkillsAsync(string userId, UpdateUserSkillsRequest skillsRequest, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var currentSkills = await _context.UserSkills
			.Where(us => us.UserId == userId)
			.ToListAsync(cancellationToken);

		var submittedSkillNames = skillsRequest.Skills
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrEmpty(s))
			.ToList() ?? [];
		
		var existingSkills = user.Skills
			.Where(us => us.Skill is not null)
			.Select(us => us.Skill!.Name)
			.ToList();

		var newSkills = submittedSkillNames.Except(existingSkills);

		var currentSkillNames = currentSkills
			.Where(us => us.Skill != null)
			.Select(us => us.Skill!.Name)
			.ToList();

		var removedSkillNames = currentSkillNames
			.Except(submittedSkillNames, StringComparer.OrdinalIgnoreCase)
			.ToList();

		if (newSkills.Any())
		{
			var existingSkillsInDb = await _context.Skills
				.Where(s => newSkills.Contains(s.Name))
				.ToListAsync(cancellationToken);

			var skillsToCreate = newSkills
				.Except(existingSkillsInDb.Select(s => s.Name), StringComparer.OrdinalIgnoreCase)
				.ToList();

			foreach (var skill in skillsToCreate)
			{
				_context.Skills.Add(new Skill { Name =  skill});
				existingSkillsInDb.Add(new Skill { Name = skill });
			}

			await _context.SaveChangesAsync(cancellationToken);

			var skillsToAdd = await _context.Skills
				.Where(s => newSkills.Contains(s.Name))
				.ToListAsync(cancellationToken);

			skillsToAdd.ForEach(skill => _context.UserSkills.Add(new UserSkill
			{
				UserId = user.Id,
				SkillId = skill.Id
			}));

			await _context.SaveChangesAsync(cancellationToken);
		}

		if (!submittedSkillNames.Any())
		{
			await _context.UserSkills
				.Where(us => us.UserId == userId)
				.ExecuteDeleteAsync(cancellationToken);

			await _context.SaveChangesAsync(cancellationToken);
		}

		await _context.UserSkills
			.Where(us => us.UserId == userId && removedSkillNames.Contains(us.Skill!.Name))
			.ExecuteDeleteAsync(cancellationToken);

		var response = user.Adapt<UserProfileResponse>();
		return Result.Success(response);
	}

	public async Task<Result<UserProfileResponse>> UpdateSocialLinksAsync(string userId, UpdateSocialLinksRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Include(u => u.SocialMediaLinks)
			.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var submittedLinks = request.SocialLinks
			.Where(sl => !string.IsNullOrWhiteSpace(sl.Platform) && !string.IsNullOrWhiteSpace(sl.Link))
			.Select(sl => new { Platform = sl.Platform.Trim(), Link = sl.Link.Trim() })
			.ToList();

		await _context.UserSocialMedia
			.Where(usm => usm.UserId == userId)
			.ExecuteDeleteAsync(cancellationToken);

		foreach (var link in submittedLinks)
		{
			_context.UserSocialMedia.Add(new UserSocialMedia
			{
				UserId = userId,
				Platform = link.Platform,
				Link = link.Link
			});
		}

		await _context.SaveChangesAsync(cancellationToken);

		var updatedUser = await _userManager.Users
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync(u => u.Id == userId, cancellationToken);

		var response = updatedUser.Adapt<UserProfileResponse>();
		return Result.Success(response);
	}

	public async Task<Result<UserProfileResponse>> UpdateEducationsAsync(string userId, UpdateEducationsRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Include(u => u.Educations)
			.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		await _context.UserEducations
			.Where(ue => ue.UserId == userId)
			.ExecuteDeleteAsync(cancellationToken);

		foreach (var education in request.Educations)
		{
			if (string.IsNullOrWhiteSpace(education.InstitutionName))
				continue;

			_context.UserEducations.Add(new UserEducation
			{
				UserId = userId,
				InstitutionName = education.InstitutionName.Trim(),
				Degree = education.Degree?.Trim(),
				FieldOfStudy = education.FieldOfStudy?.Trim(),
				StartDate = education.StartDate,
				EndDate = education.EndDate,
				Grade = education.Grade?.Trim(),
				Description = education.Description?.Trim(),
				CreatedAt = DateTime.UtcNow
			});
		}

		await _context.SaveChangesAsync(cancellationToken);

		var updatedUser = await _userManager.Users
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync(u => u.Id == userId, cancellationToken);

		var response = updatedUser.Adapt<UserProfileResponse>();
		return Result.Success(response);
	}

	public async Task<Result<UserProfileResponse>> UpdateCertificatesAsync(string userId, UpdateCertificatesRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Include(u => u.Certificates)
			.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		await _context.UserCertificates
			.Where(uc => uc.UserId == userId)
			.ExecuteDeleteAsync(cancellationToken);

		foreach (var certificate in request.Certificates)
		{
			if (string.IsNullOrWhiteSpace(certificate.CertificateName))
				continue;

			_context.UserCertificates.Add(new UserCertificate
			{
				UserId = userId,
				CertificateName = certificate.CertificateName.Trim(),
				Issuer = certificate.Issuer?.Trim(),
				IssueDate = certificate.IssueDate,
				ExpirationDate = certificate.ExpirationDate,
				CredentialId = certificate.CredentialId?.Trim(),
				CredentialUrl = certificate.CredentialUrl?.Trim(),
				Description = certificate.Description?.Trim(),
				CreatedAt = DateTime.UtcNow
			});
		}

		await _context.SaveChangesAsync(cancellationToken);

		var updatedUser = await _userManager.Users
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync(u => u.Id == userId, cancellationToken);

		var response = updatedUser.Adapt<UserProfileResponse>();
		return Result.Success(response);
	}

	public async Task<Result<UserProfileResponse>> UpdateUsernameAsync(string userId, UpdateUsernameRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var newUsername = request.NewUsername.Trim();

		if (user.UserName == newUsername)
			return Result.Failure<UserProfileResponse>(UserErrors.SameUsername);

		var existingUser = await _userManager.FindByNameAsync(newUsername);
		if (existingUser is not null)
			return Result.Failure<UserProfileResponse>(UserErrors.DuplicateUsername);

		user.UserName = newUsername;
		user.NormalizedUserName = newUsername.ToUpperInvariant();

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update username", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure<UserProfileResponse>(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		var updatedUser = await _userManager.Users
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync(u => u.Id == userId, cancellationToken);

		var response = updatedUser.Adapt<UserProfileResponse>();
		return Result.Success(response);
	}

	public async Task<Result<UserProfileResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByNameAsync(username);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var userProfile = await _userManager.Users
			.Where(u => u.Id == user.Id)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.ProjectToType<UserProfileResponse>()
			.SingleAsync(cancellationToken);

		return Result.Success(userProfile);
	}

	public async Task<Result<UserProfileResponse>> UpdatePhoneNumberAsync(string userId, UpdatePhoneNumberRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var newPhoneNumber = request.PhoneNumber.Trim();

		if (user.PhoneNumber == newPhoneNumber)
			return Result.Failure<UserProfileResponse>(UserErrors.SamePhoneNumber);

		var existingUser = await _userManager.Users
			.FirstOrDefaultAsync(u => u.PhoneNumber == newPhoneNumber && u.Id != userId, cancellationToken);

		if (existingUser is not null)
			return Result.Failure<UserProfileResponse>(UserErrors.DuplicatePhoneNumber);

		user.PhoneNumber = newPhoneNumber;
		user.PhoneNumberConfirmed = false;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update phone number", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure<UserProfileResponse>(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		var updatedUser = await _userManager.Users
			.Where(u => u.Id == userId)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.ProjectToType<UserProfileResponse>()
			.SingleAsync(cancellationToken);

		return Result.Success(updatedUser);
	}

	public async Task<Result<UserProfileResponse>> UpdateSummaryAsync(string userId, UpdateSummaryRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		user.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update summary", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure<UserProfileResponse>(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		var updatedUser = await _userManager.Users
			.Where(u => u.Id == userId)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.ProjectToType<UserProfileResponse>()
			.SingleAsync(cancellationToken);

		return Result.Success(updatedUser);
	}

	public async Task<Result<UserProfileResponse>> UpdateAvailabilityAsync(string userId, UpdateAvailabilityRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		if (!Enum.TryParse<UserAvailability>(request.Availability, true, out var availability))
		{
			return Result.Failure<UserProfileResponse>(
				new Error("User.InvalidAvailability", "Invalid availability status", StatusCodes.Status400BadRequest));
		}

		user.Availability = availability;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update availability", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure<UserProfileResponse>(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		var updatedUser = await _userManager.Users
			.Where(u => u.Id == userId)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.ProjectToType<UserProfileResponse>()
			.SingleAsync(cancellationToken);

		return Result.Success(updatedUser);
	}
}