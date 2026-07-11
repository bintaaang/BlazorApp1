using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class Tenant : BaseEntity
{
    [Display(Name = "Business Name")]
    [Required(ErrorMessage = "Business name wajib diisi")]
    public string BusinessName { get; set; } = string.Empty;

    [Display(Name = "Slug")]
    [Required(ErrorMessage = "Slug wajib diisi")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Owner Name")]
    public string OwnerName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<CustomerData> Customers { get; set; } = [];
    public ICollection<ServiceItem> ServiceItems { get; set; } = [];
    public ICollection<WorkOrder> WorkOrders { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<TenantSubscription> Subscriptions { get; set; } = [];
}
