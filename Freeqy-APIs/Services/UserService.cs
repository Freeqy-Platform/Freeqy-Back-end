using Freeqy_APIs.Contracts.Tracks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Freeqy_APIs.Services;

public class UserService(
	UserManager<ApplicationUser> userManager, 
	IWebHostEnvironment environment, 
	ApplicationDbContext context,
	IEmailSender emailSender,
	ILogger<UserService> logger,
	IHttpContextAccessor httpContextAccessor) : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager = userManager;
	private readonly IWebHostEnvironment _environment = environment;
	private readonly ApplicationDbContext _context = context;
	private readonly IEmailSender _emailSender = emailSender;
	private readonly ILogger<UserService> _logger = logger;
	private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
	private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
	private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

	public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
	{
		var user = await _userManager.Users
			.Where(u => u.Id == userId)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync();

		return Result.Success(BuildUserProfileResponse(user));
	}

	public async Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateUserProfileRequest request)
	{
		var user = await _userManager.Users
			.Include(u => u.Track)
			.SingleOrDefaultAsync(u => u.Id == userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var hasChanges = false;

		if (!string.IsNullOrWhiteSpace(request.FirstName) && user.FirstName != request.FirstName)
		{
			user.FirstName = request.FirstName.Trim();
			hasChanges = true;
		}

		if (!string.IsNullOrWhiteSpace(request.LastName) && user.LastName != request.LastName)
		{
			user.LastName = request.LastName.Trim();
			hasChanges = true;
		}

		if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber)
		{
			var newPhoneNumber = request.PhoneNumber.Trim();
			var existingUser = await _userManager.Users
				.FirstOrDefaultAsync(u => u.PhoneNumber == newPhoneNumber && u.Id != userId);

			if (existingUser is not null)
				return Result.Failure<UserProfileResponse>(UserErrors.DuplicatePhoneNumber);

			user.PhoneNumber = newPhoneNumber;
			user.PhoneNumberConfirmed = false;
			hasChanges = true;
		}

		if (request.Summary is not null && user.Summary != request.Summary)
		{
			user.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();
			hasChanges = true;
		}

		if (!string.IsNullOrWhiteSpace(request.Availability))
		{
			if (Enum.TryParse<UserAvailability>(request.Availability, true, out var availability) && user.Availability != availability)
			{
				user.Availability = availability;
				hasChanges = true;
			}
		}

		if (!string.IsNullOrWhiteSpace(request.TrackName))
		{
			var trackName = request.TrackName.Trim();
			var track = await _context.Tracks
				.FirstOrDefaultAsync(t => t.Name.ToLower() == trackName.ToLower());

			if (track is not null && user.TrackId != track.Id)
			{
				user.TrackId = track.Id;
				hasChanges = true;
			}
			else if (track is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("Track.NotFound", 
					         $"Track '{trackName}' does not exist. Use the dedicated track endpoints to request a new track.", 
					         StatusCodes.Status404NotFound));
			}
		}

		if (!hasChanges)
		{
			var updatedUser = await _userManager.Users
				.Where(u => u.Id == userId)
				.Include(u => u.Certificates)
				.Include(u => u.Educations)
				.Include(u => u.SocialMediaLinks)
				.Include(u => u.Skills)
				.ThenInclude(us => us.Skill)
				.Include(u => u.Track)
				.SingleAsync();

			return Result.Success(BuildUserProfileResponse(updatedUser));
		}

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

		var response = await _userManager.Users
			.Where(u => u.Id == userId)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync();

		return Result.Success(BuildUserProfileResponse(response));
	}

	private UserProfileResponse BuildUserProfileResponse(ApplicationUser user)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		string? photoUrl = null;
		string? bannerPhotoUrl = null;

		if (httpContext?.Request is not null)
		{
			var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
			
			if (!string.IsNullOrEmpty(user.PhotoUrl))
			{
				photoUrl = $"{baseUrl}{user.PhotoUrl}";
			}
			
			if (!string.IsNullOrEmpty(user.BannerPhotoUrl))
			{
				bannerPhotoUrl = $"{baseUrl}{user.BannerPhotoUrl}";
			}
		}
		else
		{
			photoUrl = user.PhotoUrl;
			bannerPhotoUrl = user.BannerPhotoUrl;
		}

		return new UserProfileResponse(
			user.Id,
			user.Email ?? string.Empty,
			user.UserName ?? string.Empty,
			user.FirstName,
			user.LastName,
			photoUrl,
			bannerPhotoUrl,
			user.PhoneNumber,
			user.Summary,
			user.Availability.ToString(),
			user.Track?.Name,
			user.Skills.Select(us => us.Skill).Adapt<IEnumerable<SkillResponse>>(),
			user.SocialMediaLinks.Select(sm => new SocialMediaLinkDto(sm.Platform, sm.Link)),
			user.Educations.Select(e => new EducationDto(
				e.Id,
				e.InstitutionName,
				e.Degree,
				e.FieldOfStudy,
				e.StartDate,
				e.EndDate,
				e.Grade,
				e.Description
			)),
			user.Certificates.Select(c => new CertificateDto(
				c.Id,
				c.CertificateName,
				c.Issuer,
				c.IssueDate,
				c.ExpirationDate,
				c.CredentialId,
				c.CredentialUrl,
				c.Description
			))
		);
	}

	public async Task<Result<UserProfileResponse>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Where(u => u.Id == userId)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleOrDefaultAsync(cancellationToken);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		return Result.Success(BuildUserProfileResponse(user));
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
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.ToListAsync(cancellationToken);

		var userResponses = users.Select(BuildUserProfileResponse);

		return Result.Success<IEnumerable<UserProfileResponse>>(userResponses);
	}

	public async Task<Result<string>> GetUserPhotoUrlAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<string>(UserErrors.UserNotFound);

		if (string.IsNullOrEmpty(user.PhotoUrl))
			return Result.Failure<string>(UserErrors.PhotoNotFound);

		var request = _httpContextAccessor.HttpContext?.Request;
		if (request is not null)
		{
			var baseUrl = $"{request.Scheme}://{request.Host}";
			var fullPhotoUrl = $"{baseUrl}{user.PhotoUrl}";
			return Result.Success(fullPhotoUrl);
		}

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

		// 10. Build full photo URL
		var fullPhotoUrl = photoUrl;
		var request = _httpContextAccessor.HttpContext?.Request;
		if (request is not null)
		{
			var baseUrl = $"{request.Scheme}://{request.Host}";
			fullPhotoUrl = $"{baseUrl}{photoUrl}";
		}

		// 11. Return success response
		var response = new UploadPhotoResponse(fullPhotoUrl, "Profile photo uploaded successfully");
		return Result.Success(response);
	}

	public async Task<Result> DeleteUserPhotoAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure(UserErrors.UserNotFound);

		if (string.IsNullOrEmpty(user.PhotoUrl))
			return Result.Failure(UserErrors.PhotoNotFound);

		var photoPath = Path.Combine(_environment.WebRootPath, user.PhotoUrl.TrimStart('/'));
		
		if (File.Exists(photoPath))
		{
			try
			{
				File.Delete(photoPath);
			}
			catch (Exception)
			{
				// Continue even if file deletion fails
			}
		}

		user.PhotoUrl = null;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure(
					new Error("User.UpdateFailed", "Failed to delete user photo", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		return Result.Success();
	}

	public async Task<Result<string>> GetUserBannerPhotoUrlAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<string>(UserErrors.UserNotFound);

		if (string.IsNullOrEmpty(user.BannerPhotoUrl))
			return Result.Failure<string>(UserErrors.BannerPhotoNotFound);

		var request = _httpContextAccessor.HttpContext?.Request;
		if (request is not null)
		{
			var baseUrl = $"{request.Scheme}://{request.Host}";
			var fullBannerPhotoUrl = $"{baseUrl}{user.BannerPhotoUrl}";
			return Result.Success(fullBannerPhotoUrl);
		}

		return Result.Success(user.BannerPhotoUrl);
	}

	public async Task<Result<UploadPhotoResponse>> UploadUserBannerPhotoAsync(string userId, IFormFile bannerPhoto)
	{
		// 1. Validate banner photo file
		if (bannerPhoto is null || bannerPhoto.Length == 0)
			return Result.Failure<UploadPhotoResponse>(UserErrors.NoBannerPhotoProvided);

		// 2. Check file size
		if (bannerPhoto.Length > MaxFileSize)
			return Result.Failure<UploadPhotoResponse>(UserErrors.PhotoFileTooLarge);

		// 3. Check file extension
		var extension = Path.GetExtension(bannerPhoto.FileName).ToLowerInvariant();
		if (!AllowedExtensions.Contains(extension))
			return Result.Failure<UploadPhotoResponse>(UserErrors.InvalidPhotoFile);

		// 4. Get user
		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
			return Result.Failure<UploadPhotoResponse>(UserErrors.UserNotFound);

		// 5. Delete old banner photo if exists
		if (!string.IsNullOrEmpty(user.BannerPhotoUrl))
		{
			var oldBannerPhotoPath = Path.Combine(_environment.WebRootPath, user.BannerPhotoUrl.TrimStart('/'));
			if (File.Exists(oldBannerPhotoPath))
			{
				File.Delete(oldBannerPhotoPath);
			}
		}

		// 6. Generate unique filename using GUID
		var fileName = $"{Guid.CreateVersion7()}{extension}";
		var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "banner-photos");
		var filePath = Path.Combine(uploadsFolder, fileName);

		// 7. Ensure directory exists
		Directory.CreateDirectory(uploadsFolder);

		// 8. Save file to disk
		await using (var fileStream = new FileStream(filePath, FileMode.Create))
		{
			await bannerPhoto.CopyToAsync(fileStream);
		}

		// 9. Update user's BannerPhotoUrl
		var bannerPhotoUrl = $"/uploads/banner-photos/{fileName}";
		user.BannerPhotoUrl = bannerPhotoUrl;

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

		// 10. Build full banner photo URL
		var fullBannerPhotoUrl = bannerPhotoUrl;
		var request = _httpContextAccessor.HttpContext?.Request;
		if (request is not null)
		{
			var baseUrl = $"{request.Scheme}://{request.Host}";
			fullBannerPhotoUrl = $"{baseUrl}{bannerPhotoUrl}";
		}

		// 11. Return success response
		var response = new UploadPhotoResponse(fullBannerPhotoUrl, "Banner photo uploaded successfully");
		return Result.Success(response);
	}

	public async Task<Result> DeleteUserBannerPhotoAsync(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure(UserErrors.UserNotFound);

		if (string.IsNullOrEmpty(user.BannerPhotoUrl))
			return Result.Failure(UserErrors.BannerPhotoNotFound);

		var bannerPhotoPath = Path.Combine(_environment.WebRootPath, user.BannerPhotoUrl.TrimStart('/'));
		
		if (File.Exists(bannerPhotoPath))
		{
			try
			{
				File.Delete(bannerPhotoPath);
			}
			catch (Exception)
			{
				// Continue even if file deletion fails
			}
		}

		user.BannerPhotoUrl = null;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure(
					new Error("User.UpdateFailed", "Failed to delete banner photo", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		return Result.Success();
	}

	public async Task<Result<UserProfileResponse>> UpdateSkillsAsync(string userId, UpdateUserSkillsRequest skillsRequest, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Track)
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

		var updatedUser = await _userManager.Users
			.Where(u => u.Id == userId)
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync(cancellationToken);

		return Result.Success(BuildUserProfileResponse(updatedUser));
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
			.Include(u => u.Certificates)
			.Include(u => u.Educations)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync(u => u.Id == userId, cancellationToken);

		return Result.Success(BuildUserProfileResponse(updatedUser));
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
			.Include(u => u.Certificates)
			.Include(u => u.SocialMediaLinks)
			.Include(u => u.Skills)
			.ThenInclude(us => us.Skill)
			.Include(u => u.Track)
			.SingleAsync(u => u.Id == userId, cancellationToken);

		return Result.Success(BuildUserProfileResponse(updatedUser));
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

	public async Task<Result<UserProfileResponse>> UpdateEmailAsync(string userId, UpdateEmailRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var newEmail = request.NewEmail.Trim().ToLowerInvariant();

		if (user.Email?.ToLowerInvariant() == newEmail)
			return Result.Failure<UserProfileResponse>(UserErrors.SameEmail);

		var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
		if (!passwordValid)
			return Result.Failure<UserProfileResponse>(UserErrors.InvalidPassword);

		var existingUser = await _userManager.FindByEmailAsync(newEmail);
		if (existingUser is not null)
			return Result.Failure<UserProfileResponse>(UserErrors.DuplicateEmail);

		var oldEmail = user.Email;
		user.Email = newEmail;
		user.NormalizedEmail = newEmail.ToUpperInvariant();
		user.EmailConfirmed = false;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update email", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure<UserProfileResponse>(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		try
		{
			var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			
			if (_emailSender is EmailService emailService)
			{
				await emailService.SendEmailConfirmationAsync(newEmail, confirmToken, userId);
				
				if (!string.IsNullOrEmpty(oldEmail))
				{
					await emailService.SendEmailChangeNotificationAsync(oldEmail, newEmail);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email confirmation for user {UserId}", userId);
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

	public async Task<Result> UpdatePasswordAsync(string userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure(UserErrors.UserNotFound);

		var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
		if (!passwordValid)
			return Result.Failure(UserErrors.InvalidPassword);

		var isSamePassword = await _userManager.CheckPasswordAsync(user, request.NewPassword);
		if (isSamePassword)
			return Result.Failure(UserErrors.SamePassword);

		var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure(
					new Error("User.UpdateFailed", "Failed to update password", StatusCodes.Status500InternalServerError));
			}

			return Result.Failure(
				new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		try
		{
			if (_emailSender is EmailService emailService && !string.IsNullOrEmpty(user.Email))
			{
				var changeTime = DateTime.UtcNow.ToString("dddd, MMMM dd, yyyy 'at' HH:mm:ss UTC");
				
				var httpContext = _httpContextAccessor.HttpContext;
				var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
				var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";
				
				await emailService.SendPasswordChangedNotificationAsync(
					user.Email, 
					changeTime, 
					ipAddress, 
					userAgent);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send password changed notification for user {UserId}", userId);
		}

		return Result.Success();
	}

	public async Task<Result> ConfirmEmailChangeAsync(string userId, string token, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure(UserErrors.UserNotFound);

		var result = await _userManager.ConfirmEmailAsync(user, token);

		if (!result.Succeeded)
		{
			_logger.LogWarning("Failed to confirm email for user {UserId}", userId);
			return Result.Failure(UserErrors.InvalidToken);
		}

		_logger.LogInformation("Email confirmed successfully for user {UserId}", userId);
		return Result.Success();
	}

	public async Task<Result<UserProfileResponse>> UpdateTrackAsync(string userId, UpdateTrackRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var trackName = request.TrackName.Trim();
		
		// Try to find exact match
		var track = await _context.Tracks
			.FirstOrDefaultAsync(t => t.Name.ToLower() == trackName.ToLower(), cancellationToken);
		
		if (track is null)
		{
			// Find similar tracks
			var allTracks = await _context.Tracks
				.AsNoTracking()
				.ToListAsync(cancellationToken);
			
			var similarTracks = allTracks
				.Where(t => 
					t.Name.Contains(trackName, StringComparison.OrdinalIgnoreCase) ||
					trackName.Contains(t.Name, StringComparison.OrdinalIgnoreCase) ||
					LevenshteinDistance(t.Name.ToLower(), trackName.ToLower()) <= 3)
				.Select(t => t.Name)
				.Take(5)
				.ToList();

			if (similarTracks.Any())
			{
				var suggestionsMessage = $"Track '{trackName}' not found. Did you mean: {string.Join(", ", similarTracks)}? " +
				                        $"Or send the same request again with confirmCreate=true to create this new track.";
				
				return Result.Failure<UserProfileResponse>(
					new Error("Track.NotFoundWithSuggestions", 
					         suggestionsMessage, 
					         StatusCodes.Status404NotFound));
			}

			// No similar tracks found - suggest creating
			return Result.Failure<UserProfileResponse>(
				new Error("Track.NotFound", 
				         $"Track '{trackName}' does not exist. Send the request again with confirmCreate=true to create this new track.", 
				         StatusCodes.Status404NotFound));
		}

		// Check if already the same
		if (user.TrackId == track.Id)
		{
			var currentProfile = await _userManager.Users
				.Where(u => u.Id == userId)
				.Include(u => u.Certificates)
				.Include(u => u.Educations)
				.Include(u => u.SocialMediaLinks)
				.Include(u => u.Skills)
				.ThenInclude(us => us.Skill)
				.Include(u => u.Track)
				.ProjectToType<UserProfileResponse>()
				.SingleAsync(cancellationToken);

			return Result.Success(currentProfile);
		}

		user.TrackId = track.Id;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update user track", StatusCodes.Status500InternalServerError));
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

	public async Task<Result<UserProfileResponse>> UpdateTrackWithConfirmAsync(
		string userId, 
		UpdateTrackWithConfirmRequest request, 
		CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var trackName = request.TrackName.Trim();
		
		// Try to find exact match
		var track = await _context.Tracks
			.FirstOrDefaultAsync(t => t.Name.ToLower() == trackName.ToLower(), cancellationToken);
		
		// If track doesn't exist and user wants to create
		if (track is null && request.ConfirmCreate)
		{
			// Instead of creating directly, create a track request
			var createRequestResult = await CreateTrackRequestAsync(
				userId, 
				new CreateTrackRequestDto(trackName), 
				cancellationToken);
			
			if (createRequestResult.IsFailure)
			{
				return Result.Failure<UserProfileResponse>(createRequestResult.Error);
			}
			
			return Result.Failure<UserProfileResponse>(
				new Error("Track.RequestSubmitted", 
				         $"Your request for track '{trackName}' has been submitted and will be reviewed by our team. You'll be notified once it's approved.", 
				         StatusCodes.Status202Accepted));
		}
		else if (track is null)
		{
			// Find similar tracks for suggestion
			var allTracks = await _context.Tracks
				.AsNoTracking()
				.ToListAsync(cancellationToken);
			
			var similarTracks = allTracks
				.Where(t => 
					t.Name.Contains(trackName, StringComparison.OrdinalIgnoreCase) ||
					trackName.Contains(t.Name, StringComparison.OrdinalIgnoreCase) ||
					LevenshteinDistance(t.Name.ToLower(), trackName.ToLower()) <= 3)
				.Select(t => t.Name)
				.Take(5)
				.ToList();

			if (similarTracks.Any())
			{
				var suggestionsMessage = $"Track '{trackName}' not found. Did you mean: {string.Join(", ", similarTracks)}? " +
				                        $"Or send the same request with confirmCreate=true to submit a request for this new track.";
				
				return Result.Failure<UserProfileResponse>(
					new Error("Track.NotFoundWithSuggestions", 
					         suggestionsMessage, 
					         StatusCodes.Status404NotFound));
			}

			// No similar tracks - inform about request option
			return Result.Failure<UserProfileResponse>(
				new Error("Track.NotFound", 
				         $"Track '{trackName}' does not exist. Send the request with confirmCreate=true to submit a request for it.", 
				         StatusCodes.Status404NotFound));
		}

		// Check if already the same
		if (user.TrackId == track.Id)
		{
			var currentProfile = await _userManager.Users
				.Where(u => u.Id == userId)
				.Include(u => u.Certificates)
				.Include(u => u.Educations)
				.Include(u => u.SocialMediaLinks)
				.Include(u => u.Skills)
				.ThenInclude(us => us.Skill)
				.Include(u => u.Track)
				.ProjectToType<UserProfileResponse>()
				.SingleAsync(cancellationToken);

			return Result.Success(currentProfile);
		}

		user.TrackId = track.Id;

		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			var error = result.Errors.FirstOrDefault();
			if (error is null)
			{
				return Result.Failure<UserProfileResponse>(
					new Error("User.UpdateFailed", "Failed to update user track", StatusCodes.Status500InternalServerError));
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

	// Helper method for fuzzy matching
	private static int LevenshteinDistance(string source, string target)
	{
		if (string.IsNullOrEmpty(source))
			return string.IsNullOrEmpty(target) ? 0 : target.Length;

		if (string.IsNullOrEmpty(target))
			return source.Length;

		var distance = new int[source.Length + 1, target.Length + 1];

		for (var i = 0; i <= source.Length; i++)
			distance[i, 0] = i;

		for (var j = 0; j <= target.Length; j++)
			distance[0, j] = j;

		for (var i = 1; i <= source.Length; i++)
		{
			for (var j = 1; j <= target.Length; j++)
			{
				var cost = target[j - 1] == source[i - 1] ? 0 : 1;

				distance[i, j] = Math.Min(
					Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
					distance[i - 1, j - 1] + cost);
			}
		}

		return distance[source.Length, target.Length];
	}

	public async Task<Result<List<TrackResponse>>> GetTracksAsync(CancellationToken cancellationToken = default)
	{
		var tracks = await _context.Tracks
			.AsNoTracking()
			.OrderBy(t => t.Name)
			.ProjectToType<TrackResponse>()
			.ToListAsync(cancellationToken);

		return Result.Success(tracks);
	}

	#region Track Request System

	public async Task<Result<TrackRequestResponse>> CreateTrackRequestAsync(
		string userId, 
		CreateTrackRequestDto request, 
		CancellationToken cancellationToken = default)
	{
		var trackName = request.TrackName.Trim();
		
		// Check if track already exists
		var existingTrack = await _context.Tracks
			.FirstOrDefaultAsync(t => t.Name.ToLower() == trackName.ToLower(), cancellationToken);
		
		if (existingTrack is not null)
		{
			return Result.Failure<TrackRequestResponse>(
				new Error("Track.AlreadyExists", 
				         $"Track '{trackName}' already exists. You can select it directly.", 
				         StatusCodes.Status400BadRequest));
		}
		
		// Check for pending request with same name
		var duplicateRequest = await _context.TrackRequests
			.FirstOrDefaultAsync(tr => 
				tr.TrackName.ToLower() == trackName.ToLower() && 
				tr.Status == TrackRequestStatus.Pending, 
				cancellationToken);
		
		if (duplicateRequest is not null)
		{
			return Result.Failure<TrackRequestResponse>(UserErrors.DuplicateTrackRequest);
		}
		
		// ✅ NEW: Monthly Rate Limiting - Check requests in current month
		var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
		var requestsThisMonth = await _context.TrackRequests
			.Where(tr => tr.RequestedBy == userId && tr.CreatedAt >= currentMonthStart)
			.CountAsync(cancellationToken);
		
		if (requestsThisMonth >= 3)
		{
			return Result.Failure<TrackRequestResponse>(
				new Error("TrackRequest.MonthlyLimitExceeded", 
				         "You have reached the maximum limit of 3 track requests per month. Please try again next month.", 
				         StatusCodes.Status429TooManyRequests));
		}
		
		// ✅ Daily Rate Limiting - Check if user has submitted a request in last 24 hours
		var yesterday = DateTime.UtcNow.AddHours(-24);
		var recentRequest = await _context.TrackRequests
			.Where(tr => tr.RequestedBy == userId && tr.CreatedAt >= yesterday)
			.FirstOrDefaultAsync(cancellationToken);
		
		if (recentRequest is not null)
		{
			return Result.Failure<TrackRequestResponse>(
				new Error("TrackRequest.DailyLimitExceeded", 
				         "You can only submit one track request per day. Please try again tomorrow.", 
				         StatusCodes.Status429TooManyRequests));
		}
		
		// Create track request
		var trackRequest = new TrackRequest
		{
			RequestedBy = userId,
			TrackName = trackName,
			Status = TrackRequestStatus.Pending,
			CreatedAt = DateTime.UtcNow
		};
		
		_context.TrackRequests.Add(trackRequest);
		await _context.SaveChangesAsync(cancellationToken);
		
		_logger.LogInformation("Track request created: {TrackName} by user {UserId}. Monthly count: {Count}/3", 
		                      trackName, userId, requestsThisMonth + 1);
		
		var response = new TrackRequestResponse(
			trackRequest.Id,
			trackRequest.TrackName,
			trackRequest.Status.ToString(),
			trackRequest.CreatedAt,
			null,
			null
		);
		
		return Result.Success(response);
	}

	public async Task<Result<TrackRequestListResponse>> GetUserTrackRequestsAsync(
		string userId, 
		CancellationToken cancellationToken = default)
	{
		var requests = await _context.TrackRequests
			.Where(tr => tr.RequestedBy == userId)
			.Include(tr => tr.MergedIntoTrack)
			.OrderByDescending(tr => tr.CreatedAt)
			.ToListAsync(cancellationToken);
		
		var response = requests.Select(tr => new TrackRequestResponse(
			tr.Id,
			tr.TrackName,
			tr.Status.ToString(),
			tr.CreatedAt,
			tr.RejectionReason,
			tr.MergedIntoTrack?.Name
		)).ToList();
		
		var listResponse = new TrackRequestListResponse(
			response,
			requests.Count,
			requests.Count(r => r.Status == TrackRequestStatus.Pending),
			requests.Count(r => r.Status == TrackRequestStatus.Approved),
			requests.Count(r => r.Status == TrackRequestStatus.Rejected)
		);
		
		return Result.Success(listResponse);
	}

	public async Task<Result<UserTrackRequestStatsResponse>> GetUserTrackRequestStatsAsync(
		string userId, 
		CancellationToken cancellationToken = default)
	{
		// Current month start date
		var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
		
		// Count requests in current month
		var monthlyRequestsUsed = await _context.TrackRequests
			.Where(tr => tr.RequestedBy == userId && tr.CreatedAt >= currentMonthStart)
			.CountAsync(cancellationToken);
		
		// Last request date
		var lastRequest = await _context.TrackRequests
			.Where(tr => tr.RequestedBy == userId)
			.OrderByDescending(tr => tr.CreatedAt)
			.FirstOrDefaultAsync(cancellationToken);
		
		// Can request today?
		var yesterday = DateTime.UtcNow.AddHours(-24);
		var canRequestToday = lastRequest == null || lastRequest.CreatedAt < yesterday;
		
		// Next available date
		DateTime? nextAvailableDate = null;
		if (lastRequest != null && !canRequestToday)
		{
			nextAvailableDate = lastRequest.CreatedAt.AddHours(24);
		}
		
		var stats = new UserTrackRequestStatsResponse(
			MonthlyRequestsUsed: monthlyRequestsUsed,
			MonthlyRequestsRemaining: Math.Max(0, 3 - monthlyRequestsUsed),
			MonthlyLimit: 3,
			CanRequestToday: canRequestToday && monthlyRequestsUsed < 3,
			LastRequestDate: lastRequest?.CreatedAt,
			NextAvailableDate: nextAvailableDate
		);
		
		return Result.Success(stats);
	}

	public async Task<Result<TrackRequestListResponse>> GetAllTrackRequestsAsync(
		TrackRequestStatus? status, 
		CancellationToken cancellationToken = default)
	{
		var query = _context.TrackRequests
			.Include(tr => tr.User)
			.Include(tr => tr.MergedIntoTrack)
			.AsQueryable();
		
		if (status.HasValue)
		{
			query = query.Where(tr => tr.Status == status.Value);
		}
		
		var requests = await query
			.OrderByDescending(tr => tr.CreatedAt)
			.ToListAsync(cancellationToken);
		
		var response = requests.Select(tr => new TrackRequestResponse(
			tr.Id,
			tr.TrackName,
			tr.Status.ToString(),
			tr.CreatedAt,
			tr.RejectionReason,
			tr.MergedIntoTrack?.Name
		)).ToList();
		
		var listResponse = new TrackRequestListResponse(
			response,
			requests.Count,
			requests.Count(r => r.Status == TrackRequestStatus.Pending),
			requests.Count(r => r.Status == TrackRequestStatus.Approved),
			requests.Count(r => r.Status == TrackRequestStatus.Rejected)
		);
		
		return Result.Success(listResponse);
	}

	public async Task<Result> ApproveTrackRequestAsync(
		string adminId, 
		ApproveTrackRequestDto request, 
		CancellationToken cancellationToken = default)
	{
		var trackRequest = await _context.TrackRequests
			.FirstOrDefaultAsync(tr => tr.Id == request.RequestId, cancellationToken);
		
		if (trackRequest is null)
		{
			return Result.Failure(UserErrors.TrackRequestNotFound);
		}
		
		if (trackRequest.Status != TrackRequestStatus.Pending)
		{
			return Result.Failure(UserErrors.TrackRequestAlreadyProcessed);
		}
		
		if (request.CreateNewTrack)
		{
			// Create new track
			var newTrack = new Track { Name = trackRequest.TrackName };
			_context.Tracks.Add(newTrack);
			await _context.SaveChangesAsync(cancellationToken);
			
			trackRequest.MergedIntoTrackId = newTrack.Id;
			_logger.LogInformation("New track '{TrackName}' created from request {RequestId}", 
			                      newTrack.Name, request.RequestId);
		}
		else if (request.MergeIntoTrackId.HasValue)
		{
			// Merge into existing track
			var existingTrack = await _context.Tracks.FindAsync(request.MergeIntoTrackId.Value);
			if (existingTrack is null)
			{
				return Result.Failure(
					new Error("Track.NotFound", "Target track not found", StatusCodes.Status404NotFound));
			}
			
			trackRequest.MergedIntoTrackId = request.MergeIntoTrackId.Value;
			_logger.LogInformation("Track request {RequestId} merged into track '{TrackName}'", 
			                      request.RequestId, existingTrack.Name);
		}
		
		trackRequest.Status = request.CreateNewTrack ? TrackRequestStatus.Approved : TrackRequestStatus.Merged;
		trackRequest.ApprovedBy = adminId;
		trackRequest.ApprovedAt = DateTime.UtcNow;
		
		await _context.SaveChangesAsync(cancellationToken);
		
		return Result.Success();
	}

	public async Task<Result> RejectTrackRequestAsync(
		string adminId, 
		RejectTrackRequestDto request, 
		CancellationToken cancellationToken = default)
	{
		var trackRequest = await _context.TrackRequests
			.FirstOrDefaultAsync(tr => tr.Id == request.RequestId, cancellationToken);
		
		if (trackRequest is null)
		{
			return Result.Failure(UserErrors.TrackRequestNotFound);
		}
		
		if (trackRequest.Status != TrackRequestStatus.Pending)
		{
			return Result.Failure(UserErrors.TrackRequestAlreadyProcessed);
		}
		
		trackRequest.Status = TrackRequestStatus.Rejected;
		trackRequest.RejectionReason = request.RejectionReason;
		trackRequest.ApprovedBy = adminId;
		trackRequest.ApprovedAt = DateTime.UtcNow;
		
		await _context.SaveChangesAsync(cancellationToken);
		
		_logger.LogInformation("Track request {RequestId} rejected by admin {AdminId}", 
		                      request.RequestId, adminId);
		
		return Result.Success();
	}

	#endregion
}