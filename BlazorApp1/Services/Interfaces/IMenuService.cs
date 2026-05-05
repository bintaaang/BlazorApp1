using BlazorApp1.Models.DTOs;

namespace BlazorApp1.Services.Interfaces;

public interface IMenuService
{
    Task<List<MenuDto>> GetMenusByUserAsync(int userId);
    Task<List<MenuDto>> GetAllMenusAsync();
}
