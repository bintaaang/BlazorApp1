using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models;

public class Menu : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int ModuleId { get; set; }
    public int Order { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Module? Module { get; set; }
    public Menu? Parent { get; set; }
    public ICollection<Menu> Children { get; set; } = [];

    [NotMapped]
    public string ModuleName { get; set; } = string.Empty;

    [NotMapped]
    public string? ModuleIcon { get; set; }

    [NotMapped]
    public int ModuleOrder { get; set; }
}
