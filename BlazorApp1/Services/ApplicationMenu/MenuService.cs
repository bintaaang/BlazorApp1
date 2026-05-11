using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.ApplicationMenu;

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
            .ThenBy(menu => menu.Order)
            .Select(menu => new Menu
            {
                Id = menu.Id,
                Name = menu.Name,
                Url = menu.Url,
                Icon = menu.Icon,
                ParentId = menu.ParentId,
                ModuleId = menu.ModuleId,
                ModuleName = menu.Module!.Name,
                ModuleIcon = menu.Module.Icon,
                ModuleOrder = menu.Module.Order,
                Order = menu.Order,
                PermissionName = menu.PermissionName
            })
            .ToListAsync();

        return menus;
    }
}
