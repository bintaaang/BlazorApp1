using BlazorApp1.Data;
using BlazorApp1.Models.Entities;
using BlazorApp1.Models.DTOs;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Services.Implementations;

public class UserService : IUserService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPermissionService _permissionService;

    public UserService(IDbContextFactory<AppDbContext> contextFactory, IPermissionService permissionService)
    {
        _contextFactory = contextFactory;
        _permissionService = permissionService;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);

        return MapToUserDto(user, permissions);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

        return MapToUserDto(user, permissions);
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var users = await context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r!.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(u => u.Username)
            .ToListAsync();

        return users
            .Select(user => MapToUserDto(
                user,
                user.UserRoles
                    .SelectMany(ur => ur.Role?.RolePermissions ?? [])
                    .Select(rp => rp.Permission?.Name)
                    .Where(permission => !string.IsNullOrWhiteSpace(permission))
                    .Distinct()
                    .Select(permission => permission!)
                    .ToList()))
            .ToList();
    }

    public async Task<List<string>> GetRoleNamesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => role.Name)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(UserDto request)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var validationResult = await ValidateRequestAsync(context, request);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
            if (role == null)
            {
                return (false, "Role tidak ditemukan");
            }

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                FullName = request.FullName.Trim(),
                PasswordHash = HashPassword(request.Password),
                IsActive = request.IsActive
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            await SyncUserRoleAsync(context, user.Id, role.Id);
            await context.SaveChangesAsync();

            return (true, "User berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            return (false, $"Gagal menambah user: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UserDto request)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return (false, "User tidak ditemukan");
            }

            var validationResult = await ValidateRequestAsync(context, request, userId);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
            if (role == null)
            {
                return (false, "Role tidak ditemukan");
            }

            user.Username = request.Username.Trim();
            user.Email = request.Email.Trim();
            user.FullName = request.FullName.Trim();
            user.IsActive = request.IsActive;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = HashPassword(request.Password);
            }

            await SyncUserRoleAsync(context, user.Id, role.Id);
            await context.SaveChangesAsync();

            return (true, "User berhasil diperbarui");
        }
        catch (Exception ex)
        {
            return (false, $"Gagal memperbarui user: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "User tidak ditemukan");
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return (true, "User berhasil dihapus");
        }
        catch (Exception ex)
        {
            return (false, $"Gagal menghapus user: {ex.Message}");
        }
    }

    private static UserDto MapToUserDto(Models.Entities.User user, List<string> permissions)
    {
        var roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Permissions = permissions,
            Roles = roles,
            StatusText = user.IsActive ? "Aktif" : "Tidak Aktif",
            RolesText = roles.Count == 0 ? "-" : string.Join(", ", roles),
            PermissionCount = permissions.Count
        };
    }

    private static async Task<(bool Success, string Message)> ValidateRequestAsync(
        AppDbContext context,
        UserDto request,
        int? currentUserId = null)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await context.Users.AnyAsync(u => u.Username == username && u.Id != currentUserId))
        {
            return (false, "Username sudah terdaftar");
        }

        if (await context.Users.AnyAsync(u => u.Email == email && u.Id != currentUserId))
        {
            return (false, "Email sudah terdaftar");
        }

        return (true, string.Empty);
    }

    private static async Task SyncUserRoleAsync(AppDbContext context, int userId, int roleId)
    {
        var existingRoles = await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        if (existingRoles.Count == 1 && existingRoles[0].RoleId == roleId)
        {
            return;
        }

        context.UserRoles.RemoveRange(existingRoles);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
