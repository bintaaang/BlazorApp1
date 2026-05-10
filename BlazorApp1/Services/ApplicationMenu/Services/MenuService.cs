using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.ApplicationMenu.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.ApplicationMenu.Services;

public class MenuService : IMenuService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MenuService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Menu>> GetMenusByUserAsync(int userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var userPermissionNames = context.UserPermissions
            .AsNoTracking()
            .Where(userPermission => userPermission.UserId == userId)
            .Select(userPermission => userPermission.Permission!.Name);

        var isAdmin = await context.UserRoles
            .AsNoTracking()
            .AnyAsync(userRole =>
                userRole.UserId == userId &&
                userRole.Role != null &&
                userRole.Role.Name == "Admin");

        var permissionNames = userPermissionNames.Distinct();

        if (isAdmin)
        {
            var rolePermissionNames = context.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.UserId == userId && userRole.Role != null)
                .SelectMany(userRole => userRole.Role!.RolePermissions)
                .Select(rolePermission => rolePermission.Permission!.Name);

            permissionNames = rolePermissionNames
                .Union(userPermissionNames)
                .Distinct();
        }

        var menus = await context.Menus
            .AsNoTracking()
            .Where(menu =>
                menu.IsActive &&
                menu.Url != "/profile" &&
                menu.Module != null &&
                menu.Module.IsActive &&
                permissionNames.Contains(menu.PermissionName))
            .OrderBy(m => m.Module!.Order)
            .ThenBy(m => m.Order)
            .Select(m => new Menu
            {
                Id = m.Id,
                Name = m.Name,
                Url = m.Url,
                Icon = m.Icon,
                ParentId = m.ParentId,
                ModuleId = m.ModuleId,
                ModuleName = m.Module!.Name,
                ModuleIcon = m.Module.Icon,
                ModuleOrder = m.Module.Order,
                Order = m.Order,
                PermissionName = m.PermissionName
            })
            .ToListAsync();

        return menus;
    }

}
