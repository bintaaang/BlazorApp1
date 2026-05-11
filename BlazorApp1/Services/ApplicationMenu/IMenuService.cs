using BlazorApp1.Models;

namespace BlazorApp1.Services.ApplicationMenu;

public interface IMenuService
{
    Task<List<Menu>> GetMenusByUserAsync(int userId);
}
