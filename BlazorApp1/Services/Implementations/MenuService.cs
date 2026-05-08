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

        var menus = await context.UserMenus
            .AsNoTracking()
            .Where(um => um.UserId == userId && um.IsActive && um.Menu != null && um.Menu.IsActive)
            .Select(um => um.Menu!)
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
        await using var context = await _contextFactory.CreateDbContextAsync();

        var menus = await context.Menus
            .AsNoTracking()
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
