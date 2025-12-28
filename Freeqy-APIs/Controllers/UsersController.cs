using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

/// <summary>
/// Manages user profiles and account settings.
/// Provides endpoints for retrieving, updating, and managing user information including profile details, skills, education, certificates, and more.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("api")]
public class UsersController(IUserService userService) : ControllerBase
{
	private readonly IUserService _userService = userService;

	/// <summary>
	/// Retrieves the current authenticated user's profile information.
	/// </summary>
	/// <returns>The complete profile information of the authenticated user.</returns>
	/// <response code="200">User profile retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - user profile not found.</response>
	[HttpGet("me")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Info()
	{
		var result = await _userService.GetProfileAsync(User.GetUserId()!);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Updates the current authenticated user's profile information.
	/// Supports updating multiple fields in a single request: FirstName, LastName, PhoneNumber, Summary, Availability, and TrackName.
	/// All fields are optional - only provided fields will be updated.
	/// </summary>
	/// <param name="request">The updated profile information containing any combination of profile fields.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">User profile updated successfully.</response>
	/// <response code="400">Bad request - invalid profile data or duplicate phone number.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - specified track does not exist.</response>
	[HttpPut("me")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
	{
		var result = await _userService.UpdateProfileAsync(User.GetUserId()!, request);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Retrieves a specific user's profile by their ID.
	/// </summary>
	/// <param name="id">The ID of the user to retrieve.</param>
	/// <returns>The user's profile information.</returns>
	/// <response code="200">User profile retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - user not found.</response>
	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetUserById(string id)
	{
		var result = await _userService.GetUserByIdAsync(id);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Retrieves the current authenticated user's profile photo URL.
	/// </summary>
	/// <returns>The URL of the user's profile photo.</returns>
	/// <response code="200">Profile photo URL retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - photo not found.</response>
	[HttpGet("me/photo")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetMyPhoto()
	{
		var result = await _userService.GetUserPhotoUrlAsync(User.GetUserId()!);

		if (result.IsFailure)
			return result.ToProblem();

		// Return the photo URL
		return Ok(new { photoUrl = result.Value });
	}

	/// <summary>
	/// Uploads a profile photo for the current authenticated user.
	/// </summary>
	/// <param name="request">The photo file to upload (maximum size: 5MB).</param>
	/// <returns>The URL of the uploaded photo.</returns>
	/// <response code="201">Profile photo uploaded successfully.</response>
	/// <response code="400">Bad request - invalid file or file size exceeds limit.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPost("me/photo")]
	[RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
	[Consumes("multipart/form-data")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UploadMyPhoto([FromForm] UploadPhotoRequest request)
	{
		var result = await _userService.UploadUserPhotoAsync(User.GetUserId()!, request.Photo);

		return result.IsSuccess ? Created(string.Empty, result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Deletes the current authenticated user's profile photo.
	/// </summary>
	/// <returns>No content on successful deletion.</returns>
	/// <response code="204">Profile photo deleted successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - photo not found.</response>
	[HttpDelete("me/photo")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteMyPhoto()
	{
		var result = await _userService.DeleteUserPhotoAsync(User.GetUserId()!);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Retrieves the current authenticated user's banner photo URL.
	/// </summary>
	/// <returns>The URL of the user's banner photo.</returns>
	/// <response code="200">Banner photo URL retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - banner photo not found.</response>
	[HttpGet("me/banner-photo")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetMyBannerPhoto()
	{
		var result = await _userService.GetUserBannerPhotoUrlAsync(User.GetUserId()!);

		if (result.IsFailure)
			return result.ToProblem();

		return Ok(new { bannerPhotoUrl = result.Value });
	}

	/// <summary>
	/// Uploads a banner photo for the current authenticated user.
	/// </summary>
	/// <param name="request">The banner photo file to upload (maximum size: 5MB).</param>
	/// <returns>The URL of the uploaded banner photo.</returns>
	/// <response code="201">Banner photo uploaded successfully.</response>
	/// <response code="400">Bad request - invalid file or file size exceeds limit.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPost("me/banner-photo")]
	[RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
	[Consumes("multipart/form-data")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UploadMyBannerPhoto([FromForm] UploadBannerPhotoRequest request)
	{
		var result = await _userService.UploadUserBannerPhotoAsync(User.GetUserId()!, request.BannerPhoto);

		return result.IsSuccess ? Created(string.Empty, result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Deletes the current authenticated user's banner photo.
	/// </summary>
	/// <returns>No content on successful deletion.</returns>
	/// <response code="204">Banner photo deleted successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - banner photo not found.</response>
	[HttpDelete("me/banner-photo")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteMyBannerPhoto()
	{
		var result = await _userService.DeleteUserBannerPhotoAsync(User.GetUserId()!);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Retrieves all users with optional filtering.
	/// </summary>
	/// <param name="userProfileRequestFilter">The filter criteria for users (search, skills, availability, etc.).</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>A paginated list of users matching the filter criteria.</returns>
	/// <response code="200">Users retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpGet("")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetAll([FromQuery] UserProfileRequestFilter userProfileRequestFilter, CancellationToken cancellationToken)
	{
		var result = await _userService.GetAllAsync(userProfileRequestFilter, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Updates the skills for the current authenticated user.
	/// </summary>
	/// <param name="SkillsRequest">The list of skills to update.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Skills updated successfully.</response>
	/// <response code="400">Bad request - invalid skills data.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPost("me/skills")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateSkills([FromBody] UpdateUserSkillsRequest SkillsRequest, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateSkillsAsync(User.GetUserId()!, SkillsRequest, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the social media links for the current authenticated user.
	/// </summary>
	/// <param name="request">The social media links to update (LinkedIn, GitHub, Twitter, etc.).</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Social links updated successfully.</response>
	/// <response code="400">Bad request - invalid social links data.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/social-links")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateSocialLinks([FromBody] UpdateSocialLinksRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateSocialLinksAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the education history for the current authenticated user.
	/// </summary>
	/// <param name="request">The list of education entries to update.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Education history updated successfully.</response>
	/// <response code="400">Bad request - invalid education data.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/education")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateEducations([FromBody] UpdateEducationsRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateEducationsAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the certificates for the current authenticated user.
	/// </summary>
	/// <param name="request">The list of certificates to update.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Certificates updated successfully.</response>
	/// <response code="400">Bad request - invalid certificates data.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/certificates")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateCertificates([FromBody] UpdateCertificatesRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateCertificatesAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the username for the current authenticated user.
	/// </summary>
	/// <param name="request">The new username.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Username updated successfully.</response>
	/// <response code="400">Bad request - invalid username or username already taken.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/username")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateUsernameAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Searches for a user by their username.
	/// </summary>
	/// <param name="username">The username to search for.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>The user's profile information.</returns>
	/// <response code="200">User found successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - user not found.</response>
	[HttpGet("search/{username}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetUserByUsername(string username, CancellationToken cancellationToken)
	{
		var result = await _userService.GetUserByUsernameAsync(username, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Updates the phone number for the current authenticated user.
	/// </summary>
	/// <param name="request">The new phone number.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Phone number updated successfully.</response>
	/// <response code="400">Bad request - invalid phone number format.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/phone-number")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdatePhoneNumberAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the professional summary for the current authenticated user.
	/// </summary>
	/// <param name="request">The new professional summary text.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Summary updated successfully.</response>
	/// <response code="400">Bad request - invalid summary data.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/summary")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateSummary([FromBody] UpdateSummaryRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateSummaryAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the availability status for the current authenticated user.
	/// </summary>
	/// <param name="request">The new availability status (e.g., Available, Busy, Away).</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Availability updated successfully.</response>
	/// <response code="400">Bad request - invalid availability status.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/availability")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateAvailability([FromBody] UpdateAvailabilityRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateAvailabilityAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the track/specialization for the current authenticated user.
	/// </summary>
	/// <param name="request">The new track name.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Track updated successfully.</response>
	/// <response code="400">Bad request - invalid track name.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - track not found.</response>
	[HttpPut("me/track")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateTrack([FromBody] UpdateTrackRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateTrackAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the track/specialization for the current authenticated user with auto-create option.
	/// If track doesn't exist, it suggests similar tracks or allows creation with confirmCreate=true.
	/// </summary>
	/// <param name="request">The track name and confirmation flag.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Track updated successfully.</response>
	/// <response code="400">Bad request - invalid track name.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="404">Not found - track not found with suggestions.</response>
	[HttpPut("me/track/smart")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateTrackSmart([FromBody] UpdateTrackWithConfirmRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateTrackWithConfirmAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Retrieves all available tracks/specializations.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>A list of all available tracks.</returns>
	/// <response code="200">Tracks retrieved successfully.</response>
	[HttpGet("tracks")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<IActionResult> GetTracks(CancellationToken cancellationToken)
	{
		var result = await _userService.GetTracksAsync(cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Creates a new track request for review by administrators.
	/// </summary>
	/// <param name="request">The track name to request.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>The created track request details.</returns>
	/// <response code="201">Track request created successfully.</response>
	/// <response code="400">Bad request - track already exists or invalid data.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="409">Conflict - duplicate request exists.</response>
	/// <response code="429">Too many requests - rate limit exceeded.</response>
	[HttpPost("track-requests")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	public async Task<IActionResult> CreateTrackRequest([FromBody] CreateTrackRequestDto request, CancellationToken cancellationToken)
	{
		var result = await _userService.CreateTrackRequestAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Created(string.Empty, result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Retrieves all track requests submitted by the current user.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>List of user's track requests with their status.</returns>
	/// <response code="200">Track requests retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpGet("me/track-requests")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetMyTrackRequests(CancellationToken cancellationToken)
	{
		var result = await _userService.GetUserTrackRequestsAsync(User.GetUserId()!, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Retrieves track request statistics for the current user including usage limits.
	/// Shows how many requests used this month and when the next request is available.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>User's track request statistics and limits.</returns>
	/// <response code="200">Statistics retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpGet("me/track-requests/stats")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetMyTrackRequestStats(CancellationToken cancellationToken)
	{
		var result = await _userService.GetUserTrackRequestStatsAsync(User.GetUserId()!, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Retrieves all track requests (Admin only).
	/// </summary>
	/// <param name="status">Optional filter by request status.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>List of all track requests.</returns>
	/// <response code="200">Track requests retrieved successfully.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="403">Forbidden - user is not an admin.</response>
	[HttpGet("track-requests")]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> GetAllTrackRequests([FromQuery] TrackRequestStatus? status, CancellationToken cancellationToken)
	{
		var result = await _userService.GetAllTrackRequestsAsync(status, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	/// <summary>
	/// Approves a track request (Admin only).
	/// </summary>
	/// <param name="request">The approval details including whether to create new or merge.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on success.</returns>
	/// <response code="204">Track request approved successfully.</response>
	/// <response code="400">Bad request - request already processed or invalid data.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="403">Forbidden - user is not an admin.</response>
	/// <response code="404">Not found - track request not found.</response>
	[HttpPost("track-requests/approve")]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> ApproveTrackRequest([FromBody] ApproveTrackRequestDto request, CancellationToken cancellationToken)
	{
		var result = await _userService.ApproveTrackRequestAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Rejects a track request (Admin only).
	/// </summary>
	/// <param name="request">The rejection details including reason.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on success.</returns>
	/// <response code="204">Track request rejected successfully.</response>
	/// <response code="400">Bad request - request already processed.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	/// <response code="403">Forbidden - user is not an admin.</response>
	/// <response code="404">Not found - track request not found.</response>
	[HttpPost("track-requests/reject")]
	[Authorize(Roles = "Admin")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RejectTrackRequest([FromBody] RejectTrackRequestDto request, CancellationToken cancellationToken)
	{
		var result = await _userService.RejectTrackRequestAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the email address for the current authenticated user.
	/// Sends a confirmation email to the new address.
	/// </summary>
	/// <param name="request">The new email address.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update request.</returns>
	/// <response code="204">Email update initiated successfully. Confirmation email sent.</response>
	/// <response code="400">Bad request - invalid email format or email already in use.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/email")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateEmailAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Updates the password for the current authenticated user.
	/// </summary>
	/// <param name="request">The current password and new password.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>No content on successful update.</returns>
	/// <response code="204">Password updated successfully.</response>
	/// <response code="400">Bad request - invalid password or current password is incorrect.</response>
	/// <response code="401">Unauthorized - user is not authenticated.</response>
	[HttpPut("me/password")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdatePasswordAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	/// <summary>
	/// Confirms an email address change using the token sent to the new email.
	/// This endpoint does not require authentication as it's accessed via email link.
	/// </summary>
	/// <param name="userId">The ID of the user confirming the email change.</param>
	/// <param name="token">The confirmation token sent to the new email address.</param>
	/// <param name="cancellationToken">The cancellation token for the operation.</param>
	/// <returns>A confirmation message.</returns>
	/// <response code="200">Email confirmed successfully.</response>
	/// <response code="400">Bad request - invalid or expired token.</response>
	[HttpPost("confirm-email")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> ConfirmEmailChange([FromQuery] string userId, [FromQuery] string token, CancellationToken cancellationToken)
	{
		var result = await _userService.ConfirmEmailChangeAsync(userId, token, cancellationToken);

		if (result.IsSuccess)
		{
			return Ok(new { message = "Email confirmed successfully! You can now login with your new email." });
		}

		return result.ToProblem();
	}
}