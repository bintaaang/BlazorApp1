using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Administration.Permission.Interfaces;
using BlazorApp1.Services.Auth.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Services.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPermissionService _permissionService;
    public AuthService(
        IDbContextFactory<AppDbContext> contextFactory,
        IPermissionService permissionService)
    {
        _contextFactory = contextFactory;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
                return new LoginResponse { Success = false, Message = "User tidak ditemukan" };

            var passwordHash = HashPassword(request.Password);
            if (user.PasswordHash != passwordHash)
                return new LoginResponse { Success = false, Message = "Password salah" };

            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            var roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList();

            var userModel = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Permissions = permissions,
                Roles = roles
            };

            return new LoginResponse
            {
                Success = true,
                Message = "Login berhasil",
                User = userModel
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
