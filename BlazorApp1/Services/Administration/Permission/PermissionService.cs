using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Administration.Permission;

public class PermissionService : IPermissionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PermissionService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var userPermissions = context.UserPermissions
            .AsNoTracking()
            .Where(userPermission => userPermission.UserId == userId)
            .Select(userPermission => userPermission.Permission!.Name);

        if (!await IsAdminAsync(context, userId))
        {
            return await userPermissions
                .Distinct()
                .ToListAsync();
        }

        var rolePermissions = context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission!.Name);

        return await rolePermissions
            .Union(userPermissions)
            .Distinct()
            .ToListAsync();
    }

    private static Task<bool> IsAdminAsync(AppDbContext context, int userId)
    {
        return context.UserRoles
            .AsNoTracking()
            .AnyAsync(userRole =>
                userRole.UserId == userId &&
                userRole.Role != null &&
                userRole.Role.Name == "Admin");
    }
}
