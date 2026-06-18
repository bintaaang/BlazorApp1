using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Saas;

public class SaasDashboardService : ISaasDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SaasDashboardService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<SaasDashboardSummary> GetSummaryAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var activePlanRevenue = await context.TenantSubscriptions
            .AsNoTracking()
            .Where(subscription => subscription.Status == "Active" || subscription.Status == "Trial")
            .Include(subscription => subscription.SubscriptionPlan)
            .SumAsync(subscription => subscription.SubscriptionPlan == null
                ? 0
                : subscription.SubscriptionPlan.MonthlyPrice);

        var unpaidInvoiceAmount = await context.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.Status != "Paid")
            .SumAsync(invoice => invoice.TotalAmount - invoice.PaidAmount);

        return new SaasDashboardSummary
        {
            TenantCount = await context.Tenants.AsNoTracking().CountAsync(),
            ActiveTenantCount = await context.Tenants.AsNoTracking().CountAsync(tenant => tenant.IsActive),
            CustomerCount = await context.CustomerData.AsNoTracking().CountAsync(),
            WorkOrderCount = await context.WorkOrders.AsNoTracking().CountAsync(),
            OpenWorkOrderCount = await context.WorkOrders.AsNoTracking().CountAsync(order => order.Status != "Completed" && order.Status != "Canceled"),
            InvoiceCount = await context.Invoices.AsNoTracking().CountAsync(),
            MonthlyRecurringRevenue = activePlanRevenue,
            UnpaidInvoiceAmount = unpaidInvoiceAmount,
            RecentTenants = await context.Tenants
                .AsNoTracking()
                .OrderByDescending(tenant => tenant.CreatedAt)
                .Take(5)
                .ToListAsync(),
            RecentWorkOrders = await context.WorkOrders
                .AsNoTracking()
                .Include(order => order.CustomerData)
                .OrderByDescending(order => order.OrderDate)
                .Take(5)
                .ToListAsync(),
            RecentInvoices = await context.Invoices
                .AsNoTracking()
                .Include(invoice => invoice.CustomerData)
                .OrderByDescending(invoice => invoice.InvoiceDate)
                .Take(5)
                .ToListAsync()
        };
    }
}
