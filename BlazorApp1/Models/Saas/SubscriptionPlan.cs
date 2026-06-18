using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class SubscriptionPlan : BaseEntity
{
    [Display(Name = "Plan Name")]
    [Required(ErrorMessage = "Plan name wajib diisi")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Monthly Price")]
    public decimal MonthlyPrice { get; set; }

    [Display(Name = "User Limit")]
    public int UserLimit { get; set; }

    [Display(Name = "Customer Limit")]
    public int CustomerLimit { get; set; }

    [Display(Name = "Invoice Limit")]
    public int InvoiceLimit { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<TenantSubscription> Subscriptions { get; set; } = [];
}
