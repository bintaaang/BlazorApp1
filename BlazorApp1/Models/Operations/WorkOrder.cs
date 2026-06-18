using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class WorkOrder : BaseEntity
{
    public int TenantId { get; set; }
    public int? CustomerDataId { get; set; }

    [Display(Name = "Order Number")]
    [Required(ErrorMessage = "Order number wajib diisi")]
    public string OrderNumber { get; set; } = string.Empty;

    [Display(Name = "Title")]
    [Required(ErrorMessage = "Title wajib diisi")]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal EstimatedAmount { get; set; }

    public Tenant? Tenant { get; set; }
    public CustomerData? CustomerData { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = [];
}
