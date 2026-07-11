namespace BlazorApp1.Models;

public class SaasDashboardSummary
{
    public int TenantCount { get; set; }
    public int ActiveTenantCount { get; set; }
    public int CustomerCount { get; set; }
    public int WorkOrderCount { get; set; }
    public int OpenWorkOrderCount { get; set; }
    public int InvoiceCount { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal UnpaidInvoiceAmount { get; set; }
    public List<Tenant> RecentTenants { get; set; } = [];
    public List<WorkOrder> RecentWorkOrders { get; set; } = [];
    public List<Invoice> RecentInvoices { get; set; } = [];
}
