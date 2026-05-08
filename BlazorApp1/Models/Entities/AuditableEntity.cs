namespace BlazorApp1.Models.Entities;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; } = "system";
    public string? UpdatedBy { get; set; }
}
