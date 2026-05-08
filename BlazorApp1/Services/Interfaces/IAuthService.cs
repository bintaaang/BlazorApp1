using BlazorApp1.Models.DTOs;

namespace BlazorApp1.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
}
