using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.MasterData.CustomerData.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.MasterData.CustomerData.Services;

public class CustomerDataService : ICustomerDataService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CustomerDataService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<BlazorApp1.Models.CustomerData>> GetAllCustomerDataAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.CustomerData
            .AsNoTracking()
            .OrderBy(customer => customer.CustomerName)
            .ToListAsync();
    }

    public async Task<BlazorApp1.Models.CustomerData?> GetCustomerDataByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.CustomerData
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Id == id);
    }

    public async Task<string> CreateCustomerDataAsync(BlazorApp1.Models.CustomerData data)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            data.CustomerName = data.CustomerName.Trim();
            data.CustomerType = data.CustomerType.Trim();

            context.CustomerData.Add(data);
            await context.SaveChangesAsync();

            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menambah customer data: {ex.Message}";
        }
    }

    public async Task<string> UpdateCustomerDataAsync(BlazorApp1.Models.CustomerData data)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var customer = await context.CustomerData.FirstOrDefaultAsync(item => item.Id == data.Id);
            if (customer == null)
            {
                return "Customer data tidak ditemukan";
            }

            customer.CustomerName = data.CustomerName.Trim();
            customer.CustomerType = data.CustomerType.Trim();
            customer.UpdatedBy = data.UpdatedBy;

            await context.SaveChangesAsync();

            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal memperbarui customer data: {ex.Message}";
        }
    }

    public async Task<string> DeleteCustomerDataAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var customer = await context.CustomerData.FirstOrDefaultAsync(item => item.Id == id);
            if (customer == null)
            {
                return "Customer data tidak ditemukan";
            }

            context.CustomerData.Remove(customer);
            await context.SaveChangesAsync();

            return "ok";
        }
        catch (Exception ex)
        {
            return $"Gagal menghapus customer data: {ex.Message}";
        }
    }
}
