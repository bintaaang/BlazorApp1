namespace BlazorApp1.Models;

public abstract class BaseEntity : AuditableEntity
{
    public int Id { get; set; }
}
