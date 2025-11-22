namespace Freeqy_APIs.Extensions;

public static class UserExtensions
{
	public static string? GetUserId(this ClaimsPrincipal claimsPrincipal) =>
		claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
}