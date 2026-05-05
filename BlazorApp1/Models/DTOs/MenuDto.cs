namespace BlazorApp1.Models.DTOs;

public class MenuDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int Order { get; set; }
}
