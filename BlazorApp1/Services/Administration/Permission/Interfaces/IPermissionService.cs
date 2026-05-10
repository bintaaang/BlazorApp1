namespace BlazorApp1.Services.Administration.Permission.Interfaces;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(int userId);
}
