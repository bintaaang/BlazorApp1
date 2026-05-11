using BlazorApp1.Models;

namespace BlazorApp1.Services.Administration.UserManagement;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<List<User>> GetAllUsersAsync();
    Task<List<string>> GetRoleNamesAsync();
    Task<List<Menu>> GetMenuAccessOptionsAsync();
    Task<(bool Success, string Message)> CreateUserAsync(User request);
    Task<(bool Success, string Message)> UpdateUserAsync(int userId, User request);
    Task<(bool Success, string Message)> DeleteUserAsync(int userId);
}
