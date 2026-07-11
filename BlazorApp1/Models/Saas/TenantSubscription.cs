namespace BlazorApp1.Models;

public class TenantSubscription : BaseEntity
{
    public int TenantId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndsAt { get; set; }
    public string Status { get; set; } = "Trial";

    public Tenant? Tenant { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
