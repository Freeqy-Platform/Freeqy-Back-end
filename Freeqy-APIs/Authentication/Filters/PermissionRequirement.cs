using Microsoft.AspNetCore.Authorization;

namespace Freeqy_APIs.Authentication.Filters;

public class PermissionRequirement(string permission): IAuthorizationRequirement
{
    public string Permission { get; } =  permission;
}