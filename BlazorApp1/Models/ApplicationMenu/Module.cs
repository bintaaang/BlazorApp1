namespace BlazorApp1.Models;

public class Module : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Icon { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Menu> Menus { get; set; } = [];
}
