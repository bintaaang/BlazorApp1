using BlazorApp1.Models;

namespace BlazorApp1.Services.Saas;

public interface IOperationsReadService
{
    Task<List<WorkOrder>> GetRecentWorkOrdersAsync();
    Task<List<Invoice>> GetRecentInvoicesAsync();
}