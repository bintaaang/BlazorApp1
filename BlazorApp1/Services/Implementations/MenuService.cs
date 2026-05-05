using BlazorApp1.Data;
using BlazorApp1.Models.DTOs;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Implementations;

public class MenuService : IMenuService
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;

    public MenuService(AppDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<List<MenuDto>> GetMenusByUserAsync(int userId)
    {
        var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);

        var menus = await _context.Menus
            .Where(m => m.IsActive && userPermissions.Contains(m.PermissionName))
            .OrderBy(m => m.Order)
            .Select(m => new MenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Url = m.Url,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Order = m.Order
            })
            .ToListAsync();

        return menus;
    }

    public async Task<List<MenuDto>> GetAllMenusAsync()
    {
        var menus = await _context.Menus
            .Where(m => m.IsActive)
            .OrderBy(m => m.Order)
            .Select(m => new MenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Url = m.Url,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Order = m.Order
            })
            .ToListAsync();

        return menus;
    }
}
