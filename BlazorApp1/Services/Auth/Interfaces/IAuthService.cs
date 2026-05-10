using BlazorApp1.Models;

namespace BlazorApp1.Services.Auth.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
