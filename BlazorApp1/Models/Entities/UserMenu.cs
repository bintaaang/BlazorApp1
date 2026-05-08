namespace BlazorApp1.Models.Entities;

public class UserMenu : AuditableEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int MenuId { get; set; }
    public Menu? Menu { get; set; }

    public bool IsActive { get; set; } = true;
}
