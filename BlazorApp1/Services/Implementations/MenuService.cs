using BlazorApp1.Data;
using BlazorApp1.Models.DTOs;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Implementations;

public class MenuService : IMenuService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MenuService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MenuDto>> GetMenusByUserAsync(int userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var permissionNames = context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId && userRole.Role != null)
            .SelectMany(userRole => userRole.Role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission!.Name)
            .Distinct();

        var menus = await context.Menus
            .AsNoTracking()
            .Where(menu =>
                menu.IsActive &&
                menu.Module != null &&
                menu.Module.IsActive &&
                permissionNames.Contains(menu.PermissionName))
            .OrderBy(m => m.Module!.Order)
            .ThenBy(m => m.Order)
            .Select(m => new MenuDto
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
                Order = m.Order
            })
            .ToListAsync();

        return menus;
    }

}
