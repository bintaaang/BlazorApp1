using BlazorApp1.Models.DTOs;

namespace BlazorApp1.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<List<UserDto>> GetAllUsersAsync();
}
