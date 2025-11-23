namespace Freeqy_APIs.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
	private readonly IUserService _userService = userService;

	[HttpGet("me")]
	public async Task<IActionResult> Info()
	{
		var result = await _userService.GetProfileAsync(User.GetUserId()!);

		return Ok(result.Value);
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
}