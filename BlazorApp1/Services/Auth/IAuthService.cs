using BlazorApp1.Models;

namespace BlazorApp1.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
