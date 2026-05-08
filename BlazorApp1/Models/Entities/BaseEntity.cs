namespace BlazorApp1.Models.Entities;

public abstract class BaseEntity : AuditableEntity
{
    public int Id { get; set; }
}
