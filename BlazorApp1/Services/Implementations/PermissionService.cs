using BlazorApp1.Data;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Implementations;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionName)
    {
        var hasPermission = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .AnyAsync(rp => rp.Permission!.Name == permissionName);

        return hasPermission;
    }
}
