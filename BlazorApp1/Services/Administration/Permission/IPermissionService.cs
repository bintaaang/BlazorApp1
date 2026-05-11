namespace BlazorApp1.Services.Administration.Permission;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(int userId);
}
