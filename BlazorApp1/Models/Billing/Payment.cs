namespace BlazorApp1.Models;

public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Bank Transfer";
    public string ReferenceNumber { get; set; } = string.Empty;

    public Invoice? Invoice { get; set; }
}
