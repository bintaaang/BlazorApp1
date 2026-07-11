using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Saas;

public class OperationsReadService : IOperationsReadService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public OperationsReadService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<WorkOrder>> GetRecentWorkOrdersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WorkOrders
            .AsNoTracking()
            .Include(order => order.Tenant)
            .Include(order => order.CustomerData)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetRecentInvoicesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Tenant)
            .Include(invoice => invoice.CustomerData)
            .Include(invoice => invoice.WorkOrder)
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .ToListAsync();
    }
}