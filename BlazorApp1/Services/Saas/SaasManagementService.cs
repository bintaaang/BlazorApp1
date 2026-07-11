using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Saas;

public class SaasManagementService : ISaasManagementService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SaasManagementService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Tenant>> GetTenantsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Tenants
            .AsNoTracking()
            .OrderBy(tenant => tenant.BusinessName)
            .ToListAsync();
    }

    public async Task<Tenant?> GetTenantByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(tenant => tenant.Id == id);
    }

    public async Task<string> SaveTenantAsync(Tenant tenant)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            tenant.BusinessName = tenant.BusinessName.Trim();
            tenant.Slug = CreateSlug(tenant.Slug);
            tenant.OwnerName = tenant.OwnerName.Trim();
            tenant.Email = tenant.Email.Trim();
            tenant.Phone = tenant.Phone.Trim();

            var slugExists = await context.Tenants.AnyAsync(item =>
                item.Id != tenant.Id &&
                item.Slug == tenant.Slug);

            if (slugExists)
            {
                return "Slug tenant sudah digunakan.";
            }

            if (tenant.Id == 0)
            {
                context.Tenants.Add(tenant);
            }
            else
            {
                var current = await context.Tenants.FirstOrDefaultAsync(item => item.Id == tenant.Id);
                if (current == null)
                {
                    return "Tenant tidak ditemukan.";
                }

                current.BusinessName = tenant.BusinessName;
                current.Slug = tenant.Slug;
                current.OwnerName = tenant.OwnerName;
                current.Email = tenant.Email;
                current.Phone = tenant.Phone;
                current.IsActive = tenant.IsActive;
                current.UpdatedBy = tenant.UpdatedBy;
            }

            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menyimpan tenant: {ex.Message}";
        }
    }

    public async Task<string> DeleteTenantAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var hasOperationalData = await context.WorkOrders.AnyAsync(order => order.TenantId == id) ||
                await context.Invoices.AnyAsync(invoice => invoice.TenantId == id);

            if (hasOperationalData)
            {
                return "Tenant tidak bisa dihapus karena sudah memiliki transaksi.";
            }

            var tenant = await context.Tenants.FirstOrDefaultAsync(item => item.Id == id);
            if (tenant == null)
            {
                return "Tenant tidak ditemukan.";
            }

            context.Tenants.Remove(tenant);
            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menghapus tenant: {ex.Message}";
        }
    }

    public async Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(plan => plan.MonthlyPrice)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(plan => plan.Id == id);
    }

    public async Task<string> SaveSubscriptionPlanAsync(SubscriptionPlan plan)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            plan.Name = plan.Name.Trim();
            plan.Description = plan.Description.Trim();

            var nameExists = await context.SubscriptionPlans.AnyAsync(item =>
                item.Id != plan.Id &&
                item.Name == plan.Name);

            if (nameExists)
            {
                return "Nama plan sudah digunakan.";
            }

            if (plan.Id == 0)
            {
                context.SubscriptionPlans.Add(plan);
            }
            else
            {
                var current = await context.SubscriptionPlans.FirstOrDefaultAsync(item => item.Id == plan.Id);
                if (current == null)
                {
                    return "Plan tidak ditemukan.";
                }

                current.Name = plan.Name;
                current.Description = plan.Description;
                current.MonthlyPrice = plan.MonthlyPrice;
                current.UserLimit = plan.UserLimit;
                current.CustomerLimit = plan.CustomerLimit;
                current.InvoiceLimit = plan.InvoiceLimit;
                current.IsActive = plan.IsActive;
                current.UpdatedBy = plan.UpdatedBy;
            }

            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menyimpan plan: {ex.Message}";
        }
    }

    public async Task<string> DeleteSubscriptionPlanAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var isUsed = await context.TenantSubscriptions.AnyAsync(subscription => subscription.SubscriptionPlanId == id);
            if (isUsed)
            {
                return "Plan tidak bisa dihapus karena sedang digunakan tenant.";
            }

            var plan = await context.SubscriptionPlans.FirstOrDefaultAsync(item => item.Id == id);
            if (plan == null)
            {
                return "Plan tidak ditemukan.";
            }

            context.SubscriptionPlans.Remove(plan);
            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menghapus plan: {ex.Message}";
        }
    }

    public async Task<List<ServiceItem>> GetServiceItemsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.ServiceItems
            .AsNoTracking()
            .Include(item => item.Tenant)
            .OrderBy(item => item.Tenant!.BusinessName)
            .ThenBy(item => item.Name)
            .ToListAsync();
    }

    public async Task<ServiceItem?> GetServiceItemByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.ServiceItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<string> SaveServiceItemAsync(ServiceItem item)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            item.Name = item.Name.Trim();
            item.Category = item.Category.Trim();

            if (!await context.Tenants.AnyAsync(tenant => tenant.Id == item.TenantId))
            {
                return "Tenant wajib dipilih.";
            }

            if (item.Id == 0)
            {
                context.ServiceItems.Add(item);
            }
            else
            {
                var current = await context.ServiceItems.FirstOrDefaultAsync(data => data.Id == item.Id);
                if (current == null)
                {
                    return "Service item tidak ditemukan.";
                }

                current.TenantId = item.TenantId;
                current.Name = item.Name;
                current.Category = item.Category;
                current.UnitPrice = item.UnitPrice;
                current.IsActive = item.IsActive;
                current.UpdatedBy = item.UpdatedBy;
            }

            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menyimpan service item: {ex.Message}";
        }
    }

    public async Task<string> DeleteServiceItemAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var item = await context.ServiceItems.FirstOrDefaultAsync(data => data.Id == id);
            if (item == null)
            {
                return "Service item tidak ditemukan.";
            }

            context.ServiceItems.Remove(item);
            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menghapus service item: {ex.Message}";
        }
    }

    public async Task<List<WorkOrder>> GetWorkOrdersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WorkOrders
            .AsNoTracking()
            .Include(order => order.Tenant)
            .Include(order => order.CustomerData)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
    }

    public async Task<WorkOrder?> GetWorkOrderByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == id);
    }

    public async Task<string> SaveWorkOrderAsync(WorkOrder workOrder)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            workOrder.OrderNumber = workOrder.OrderNumber.Trim();
            workOrder.Title = workOrder.Title.Trim();
            workOrder.Description = workOrder.Description.Trim();
            workOrder.Status = workOrder.Status.Trim();
            workOrder.OrderDate = DateTime.SpecifyKind(workOrder.OrderDate, DateTimeKind.Utc);
            workOrder.DueDate = workOrder.DueDate.HasValue
                ? DateTime.SpecifyKind(workOrder.DueDate.Value, DateTimeKind.Utc)
                : null;

            var orderNumberExists = await context.WorkOrders.AnyAsync(order =>
                order.Id != workOrder.Id &&
                order.TenantId == workOrder.TenantId &&
                order.OrderNumber == workOrder.OrderNumber);

            if (orderNumberExists)
            {
                return "Nomor work order sudah digunakan tenant ini.";
            }

            if (workOrder.Id == 0)
            {
                context.WorkOrders.Add(workOrder);
            }
            else
            {
                var current = await context.WorkOrders.FirstOrDefaultAsync(order => order.Id == workOrder.Id);
                if (current == null)
                {
                    return "Work order tidak ditemukan.";
                }

                current.TenantId = workOrder.TenantId;
                current.CustomerDataId = workOrder.CustomerDataId;
                current.OrderNumber = workOrder.OrderNumber;
                current.Title = workOrder.Title;
                current.Description = workOrder.Description;
                current.Status = workOrder.Status;
                current.OrderDate = workOrder.OrderDate;
                current.DueDate = workOrder.DueDate;
                current.EstimatedAmount = workOrder.EstimatedAmount;
                current.UpdatedBy = workOrder.UpdatedBy;
            }

            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menyimpan work order: {ex.Message}";
        }
    }

    public async Task<string> DeleteWorkOrderAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var hasInvoice = await context.Invoices.AnyAsync(invoice => invoice.WorkOrderId == id);
            if (hasInvoice)
            {
                return "Work order tidak bisa dihapus karena sudah memiliki invoice.";
            }

            var order = await context.WorkOrders.FirstOrDefaultAsync(item => item.Id == id);
            if (order == null)
            {
                return "Work order tidak ditemukan.";
            }

            context.WorkOrders.Remove(order);
            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menghapus work order: {ex.Message}";
        }
    }

    public async Task<List<Invoice>> GetInvoicesAsync()
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

    public async Task<Invoice?> GetInvoiceByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(invoice => invoice.Id == id);
    }

    public async Task<string> SaveInvoiceAsync(Invoice invoice)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            invoice.InvoiceNumber = invoice.InvoiceNumber.Trim();
            invoice.Status = invoice.Status.Trim();
            invoice.InvoiceDate = DateTime.SpecifyKind(invoice.InvoiceDate, DateTimeKind.Utc);
            invoice.DueDate = DateTime.SpecifyKind(invoice.DueDate, DateTimeKind.Utc);
            invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount - invoice.DiscountAmount;

            var invoiceNumberExists = await context.Invoices.AnyAsync(item =>
                item.Id != invoice.Id &&
                item.TenantId == invoice.TenantId &&
                item.InvoiceNumber == invoice.InvoiceNumber);

            if (invoiceNumberExists)
            {
                return "Nomor invoice sudah digunakan tenant ini.";
            }

            if (invoice.Id == 0)
            {
                context.Invoices.Add(invoice);
            }
            else
            {
                var current = await context.Invoices.FirstOrDefaultAsync(item => item.Id == invoice.Id);
                if (current == null)
                {
                    return "Invoice tidak ditemukan.";
                }

                current.TenantId = invoice.TenantId;
                current.CustomerDataId = invoice.CustomerDataId;
                current.WorkOrderId = invoice.WorkOrderId;
                current.InvoiceNumber = invoice.InvoiceNumber;
                current.InvoiceDate = invoice.InvoiceDate;
                current.DueDate = invoice.DueDate;
                current.Subtotal = invoice.Subtotal;
                current.TaxAmount = invoice.TaxAmount;
                current.DiscountAmount = invoice.DiscountAmount;
                current.TotalAmount = invoice.TotalAmount;
                current.PaidAmount = invoice.PaidAmount;
                current.Status = invoice.Status;
                current.UpdatedBy = invoice.UpdatedBy;
            }

            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menyimpan invoice: {ex.Message}";
        }
    }

    public async Task<string> DeleteInvoiceAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var hasPayment = await context.Payments.AnyAsync(payment => payment.InvoiceId == id);
            if (hasPayment)
            {
                return "Invoice tidak bisa dihapus karena sudah memiliki pembayaran.";
            }

            var invoice = await context.Invoices.FirstOrDefaultAsync(item => item.Id == id);
            if (invoice == null)
            {
                return "Invoice tidak ditemukan.";
            }

            context.Invoices.Remove(invoice);
            await context.SaveChangesAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menghapus invoice: {ex.Message}";
        }
    }

    public async Task<List<CustomerData>> GetCustomersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.CustomerData
            .AsNoTracking()
            .OrderBy(customer => customer.CustomerName)
            .ToListAsync();
    }

    private static string CreateSlug(string value)
        => value.Trim().ToLowerInvariant().Replace(" ", "-");
}