using BlazorApp1.Data;
using BlazorApp1.Services.Administration.Permission.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Administration.Permission.Services;

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
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission!.Name);

        if (!await IsAdminAsync(context, userId))
        {
            return await userPermissions
                .Distinct()
                .ToListAsync();
        }

        var rolePermissions = context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Name);

        return await rolePermissions
            .Union(userPermissions)
            .Distinct()
            .ToListAsync();
    }

    private static Task<bool> IsAdminAsync(AppDbContext context, int userId)
    {
        return context.UserRoles
            .AsNoTracking()
            .AnyAsync(ur => ur.UserId == userId && ur.Role != null && ur.Role.Name == "Admin");
    }
}
