using BlazorApp1.Models.DTOs;

namespace BlazorApp1.Services.Interfaces;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task<bool> HasPermissionAsync(int userId, string permissionName);
}
