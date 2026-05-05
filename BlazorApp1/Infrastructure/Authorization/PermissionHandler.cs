using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BlazorApp1.Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissionClaim = context.User.FindAll("Permission");

        if (permissionClaim.Any(c => c.Value == requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
