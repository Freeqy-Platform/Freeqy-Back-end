using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Freeqy_APIs.Authentication.Filters;

public class PermissionAuthorizationHandler:  AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User.Identity as ClaimsIdentity;

        if (user == null || !user.IsAuthenticated)
        {
            return;
        }
        
        var withPermission = user.Claims.Any(x => x.Value == requirement.Permission && x.Type == "permissions");
        if (!withPermission) return;
        
        context.Succeed(requirement);

        return;
    }
}