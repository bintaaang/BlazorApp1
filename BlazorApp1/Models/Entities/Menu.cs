namespace BlazorApp1.Models.Entities;

public class Menu : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int Order { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Menu? Parent { get; set; }
    public ICollection<Menu> Children { get; set; } = [];
    public ICollection<UserMenu> UserMenus { get; set; } = [];
}
