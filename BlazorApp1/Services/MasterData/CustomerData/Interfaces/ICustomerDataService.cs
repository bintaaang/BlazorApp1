using BlazorApp1.Models;

namespace BlazorApp1.Services.MasterData.CustomerData.Interfaces;

public interface ICustomerDataService
{
    Task<List<BlazorApp1.Models.CustomerData>> GetAllCustomerDataAsync();
    Task<BlazorApp1.Models.CustomerData?> GetCustomerDataByIdAsync(int id);
    Task<string> CreateCustomerDataAsync(BlazorApp1.Models.CustomerData data);
    Task<string> UpdateCustomerDataAsync(BlazorApp1.Models.CustomerData data);
    Task<string> DeleteCustomerDataAsync(int id);
}
