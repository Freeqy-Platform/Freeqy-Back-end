using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("api")]
public class UsersController(IUserService userService) : ControllerBase
{
	private readonly IUserService _userService = userService;

	[HttpGet("me")]
	public async Task<IActionResult> Info()
	{
		var result = await _userService.GetProfileAsync(User.GetUserId()!);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me")]
	public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
	{
		var result = await _userService.UpdateProfileAsync(User.GetUserId()!, request);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetUserById(string id)
	{
		var result = await _userService.GetUserByIdAsync(id);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpGet("me/photo")]
	public async Task<IActionResult> GetMyPhoto()
	{
		var result = await _userService.GetUserPhotoUrlAsync(User.GetUserId()!);

		if (result.IsFailure)
			return result.ToProblem();

		// Return the photo URL
		return Ok(new { photoUrl = result.Value });
	}

	[HttpPost("me/photo")]
	[RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> UploadMyPhoto([FromForm] UploadPhotoRequest request)
	{
		var result = await _userService.UploadUserPhotoAsync(User.GetUserId()!, request.Photo);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpDelete("me/photo")]
	public async Task<IActionResult> DeleteMyPhoto()
	{
		var result = await _userService.DeleteUserPhotoAsync(User.GetUserId()!);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	[HttpGet("")]
	public async Task<IActionResult> GetAll([FromQuery] UserProfileRequestFilter userProfileRequestFilter, CancellationToken cancellationToken)
	{
		var result = await _userService.GetAllAsync(userProfileRequestFilter, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPost("me/skills")]
	public async Task<IActionResult> UpdateSkills([FromBody] UpdateUserSkillsRequest SkillsRequest, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateSkillsAsync(User.GetUserId()!, SkillsRequest, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me/social-links")]
	public async Task<IActionResult> UpdateSocialLinks([FromBody] UpdateSocialLinksRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateSocialLinksAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me/education")]
	public async Task<IActionResult> UpdateEducations([FromBody] UpdateEducationsRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateEducationsAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me/certificates")]
	public async Task<IActionResult> UpdateCertificates([FromBody] UpdateCertificatesRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateCertificatesAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me/username")]
	public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateUsernameAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpGet("search/{username}")]
	public async Task<IActionResult> GetUserByUsername(string username, CancellationToken cancellationToken)
	{
		var result = await _userService.GetUserByUsernameAsync(username, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me/phone-number")]
	public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdatePhoneNumberAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me/summary")]
	public async Task<IActionResult> UpdateSummary([FromBody] UpdateSummaryRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateSummaryAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPut("me/availability")]
	public async Task<IActionResult> UpdateAvailability([FromBody] UpdateAvailabilityRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.UpdateAvailabilityAsync(User.GetUserId()!, request, cancellationToken);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}
}