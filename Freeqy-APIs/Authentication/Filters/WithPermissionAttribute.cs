using Microsoft.AspNetCore.Authorization;

namespace Freeqy_APIs.Authentication.Filters;

public class WithPermissionAttribute(string permission) : AuthorizeAttribute(permission)
{
}