namespace BlazorApp1.Models.Entities;

public class Menu
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int Order { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
