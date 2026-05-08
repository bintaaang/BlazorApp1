namespace BlazorApp1.Models.Entities;

public class RolePermission : AuditableEntity
{
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public int PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
