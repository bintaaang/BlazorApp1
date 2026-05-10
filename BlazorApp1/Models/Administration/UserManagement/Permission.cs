namespace BlazorApp1.Models;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<UserPermission> UserPermissions { get; set; } = [];
}
