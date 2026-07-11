using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class ServiceItem : BaseEntity
{
    public int TenantId { get; set; }

    [Display(Name = "Service Name")]
    [Required(ErrorMessage = "Service name wajib diisi")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
}
