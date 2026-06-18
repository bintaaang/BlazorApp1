using BlazorApp1.Models;

namespace BlazorApp1.Services.Saas;

public interface ISaasManagementService
{
    Task<List<Tenant>> GetTenantsAsync();
    Task<Tenant?> GetTenantByIdAsync(int id);
    Task<string> SaveTenantAsync(Tenant tenant);
    Task<string> DeleteTenantAsync(int id);

    Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync();
    Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int id);
    Task<string> SaveSubscriptionPlanAsync(SubscriptionPlan plan);
    Task<string> DeleteSubscriptionPlanAsync(int id);

    Task<List<ServiceItem>> GetServiceItemsAsync();
    Task<ServiceItem?> GetServiceItemByIdAsync(int id);
    Task<string> SaveServiceItemAsync(ServiceItem item);
    Task<string> DeleteServiceItemAsync(int id);

    Task<List<WorkOrder>> GetWorkOrdersAsync();
    Task<WorkOrder?> GetWorkOrderByIdAsync(int id);
    Task<string> SaveWorkOrderAsync(WorkOrder workOrder);
    Task<string> DeleteWorkOrderAsync(int id);

    Task<List<Invoice>> GetInvoicesAsync();
    Task<Invoice?> GetInvoiceByIdAsync(int id);
    Task<string> SaveInvoiceAsync(Invoice invoice);
    Task<string> DeleteInvoiceAsync(int id);

    Task<List<CustomerData>> GetCustomersAsync();
}
