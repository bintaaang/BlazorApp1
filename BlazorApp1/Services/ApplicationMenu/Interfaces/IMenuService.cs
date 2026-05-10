using BlazorApp1.Models;

namespace BlazorApp1.Services.ApplicationMenu.Interfaces;

public interface IMenuService
{
    Task<List<Menu>> GetMenusByUserAsync(int userId);
}
