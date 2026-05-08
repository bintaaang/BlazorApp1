using BlazorApp1.Models.DTOs;

namespace BlazorApp1.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<List<string>> GetRoleNamesAsync();
    Task<(bool Success, string Message)> CreateUserAsync(UserDto request);
    Task<(bool Success, string Message)> UpdateUserAsync(int userId, UserDto request);
    Task<(bool Success, string Message)> DeleteUserAsync(int userId);
}
