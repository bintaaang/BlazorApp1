using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class Invoice : BaseEntity
{
    public int TenantId { get; set; }
    public int? CustomerDataId { get; set; }
    public int? WorkOrderId { get; set; }

    [Display(Name = "Invoice Number")]
    [Required(ErrorMessage = "Invoice number wajib diisi")]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "Unpaid";

    public Tenant? Tenant { get; set; }
    public CustomerData? CustomerData { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];
}
