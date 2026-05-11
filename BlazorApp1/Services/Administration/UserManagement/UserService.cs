using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Administration.Permission;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Services.Administration.UserManagement;

public class UserService : IUserService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPermissionService _permissionService;

    public UserService(IDbContextFactory<AppDbContext> contextFactory, IPermissionService permissionService)
    {
        _contextFactory = contextFactory;
        _permissionService = permissionService;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Include(user => user.UserPermissions)
            .ThenInclude(userPermission => userPermission.Permission)
            .Include(user => user.CustomerData)
            .FirstOrDefaultAsync(user => user.Id == userId);

        if (user == null)
        {
            return null;
        }

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);

        return MapToUser(user, permissions, GetUserMenuPermissions(user));
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var users = await context.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .ThenInclude(role => role!.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(user => user.UserPermissions)
            .ThenInclude(userPermission => userPermission.Permission)
            .Include(user => user.CustomerData)
            .OrderBy(user => user.Username)
            .ToListAsync();

        return users
            .Select(user =>
            {
                var roles = user.UserRoles.Select(userRole => userRole.Role?.Name).ToList();
                var userMenuPermissions = GetUserMenuPermissions(user);
                var rolePermissions = roles.Contains("Admin")
                    ? user.UserRoles
                        .SelectMany(userRole => userRole.Role?.RolePermissions ?? [])
                        .Select(rolePermission => rolePermission.Permission?.Name)
                    : [];
                var permissions = rolePermissions
                    .Concat(userMenuPermissions)
                    .Where(permission => !string.IsNullOrWhiteSpace(permission))
                    .Distinct()
                    .Select(permission => permission!)
                    .ToList();

                return MapToUser(user, permissions, userMenuPermissions);
            })
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

    public async Task<List<Menu>> GetMenuAccessOptionsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Menus
            .AsNoTracking()
            .Where(menu =>
                menu.IsActive &&
                menu.Url != "/profile" &&
                !string.IsNullOrWhiteSpace(menu.PermissionName) &&
                menu.Module != null &&
                menu.Module.IsActive)
            .OrderBy(menu => menu.Module!.Order)
            .ThenBy(menu => menu.Order)
            .Select(menu => new Menu
            {
                Id = menu.Id,
                Name = menu.Name,
                Url = menu.Url,
                Icon = menu.Icon,
                ParentId = menu.ParentId,
                ModuleId = menu.ModuleId,
                ModuleName = menu.Module!.Name,
                ModuleIcon = menu.Module.Icon,
                ModuleOrder = menu.Module.Order,
                Order = menu.Order,
                PermissionName = menu.PermissionName
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(User request)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var validationResult = await ValidateRequestAsync(context, request);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var role = await context.Roles.FirstOrDefaultAsync(role => role.Name == request.RoleName);
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
                IsActive = request.IsActive,
                CustomerDataId = request.RoleName.Equals("User", StringComparison.OrdinalIgnoreCase)
                    ? request.CustomerDataId
                    : null
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            await SyncUserRoleAsync(context, user.Id, role.Id);
            await SyncUserPermissionsAsync(context, user.Id, request);
            await context.SaveChangesAsync();

            return (true, "User berhasil ditambahkan");
        }
        catch (Exception ex)
        {
            return (false, $"Gagal menambah user: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, User request)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users
                .Include(user => user.UserRoles)
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (user == null)
            {
                return (false, "User tidak ditemukan");
            }

            var validationResult = await ValidateRequestAsync(context, request, userId);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var role = await context.Roles.FirstOrDefaultAsync(role => role.Name == request.RoleName);
            if (role == null)
            {
                return (false, "Role tidak ditemukan");
            }

            user.Username = request.Username.Trim();
            user.Email = request.Email.Trim();
            user.FullName = request.FullName.Trim();
            user.IsActive = request.IsActive;
            user.CustomerDataId = request.RoleName.Equals("User", StringComparison.OrdinalIgnoreCase)
                ? request.CustomerDataId
                : null;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = HashPassword(request.Password);
            }

            await SyncUserRoleAsync(context, user.Id, role.Id);
            await SyncUserPermissionsAsync(context, user.Id, request);
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

            var user = await context.Users.FirstOrDefaultAsync(user => user.Id == userId);
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

    private static User MapToUser(
        User user,
        List<string> permissions,
        List<string>? menuPermissions = null)
    {
        var roles = user.UserRoles.Select(userRole => userRole.Role!.Name).ToList();

        return new User
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CustomerDataId = user.CustomerDataId,
            CustomerName = user.CustomerData?.CustomerName ?? string.Empty,
            CustomerType = user.CustomerData?.CustomerType ?? string.Empty,
            Permissions = permissions,
            MenuPermissions = menuPermissions ?? [],
            Roles = roles,
            StatusText = user.IsActive ? "Aktif" : "Tidak Aktif",
            RolesText = roles.Count == 0 ? "-" : string.Join(", ", roles),
            PermissionCount = permissions.Count
        };
    }

    private static List<string> GetUserMenuPermissions(User user)
    {
        return user.UserPermissions
            .Select(userPermission => userPermission.Permission?.Name)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct()
            .Select(permission => permission!)
            .ToList();
    }

    private static async Task<(bool Success, string Message)> ValidateRequestAsync(
        AppDbContext context,
        User request,
        int? currentUserId = null)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await context.Users.AnyAsync(user => user.Username == username && user.Id != currentUserId))
        {
            return (false, "Username sudah terdaftar");
        }

        if (await context.Users.AnyAsync(user => user.Email == email && user.Id != currentUserId))
        {
            return (false, "Email sudah terdaftar");
        }

        return (true, string.Empty);
    }

    private static async Task SyncUserRoleAsync(AppDbContext context, int userId, int roleId)
    {
        var existingRoles = await context.UserRoles
            .Where(userRole => userRole.UserId == userId)
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

    private static async Task SyncUserPermissionsAsync(AppDbContext context, int userId, User request)
    {
        var existingPermissions = await context.UserPermissions
            .Where(userPermission => userPermission.UserId == userId)
            .ToListAsync();

        context.UserPermissions.RemoveRange(existingPermissions);

        if (request.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var selectedPermissionNames = request.MenuPermissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selectedPermissionNames.Count == 0)
        {
            return;
        }

        var permissionIds = await context.Permissions
            .Where(permission => selectedPermissionNames.Contains(permission.Name))
            .Select(permission => permission.Id)
            .ToListAsync();

        context.UserPermissions.AddRange(permissionIds.Select(permissionId => new UserPermission
        {
            UserId = userId,
            PermissionId = permissionId
        }));
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
