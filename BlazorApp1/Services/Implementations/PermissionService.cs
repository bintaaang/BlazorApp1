using BlazorApp1.Data;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Implementations;

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

        var permissions = await context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionName)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var hasPermission = await context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .AnyAsync(rp => rp.Permission!.Name == permissionName);

        return hasPermission;
    }
}
