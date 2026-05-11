using CustomerDataModel = BlazorApp1.Models.CustomerData;

namespace BlazorApp1.Services.MasterData.CustomerData;

public interface ICustomerDataService
{
    Task<List<CustomerDataModel>> GetAllCustomerDataAsync();
    Task<CustomerDataModel?> GetCustomerDataByIdAsync(int id);
    Task<string> CreateCustomerDataAsync(CustomerDataModel data);
    Task<string> UpdateCustomerDataAsync(CustomerDataModel data);
    Task<string> DeleteCustomerDataAsync(int id);
}
