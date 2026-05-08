using BlazorApp1.Data;
using BlazorApp1.Models.DTOs;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            .ToListAsync();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            result.Add(MapToUserDto(user, permissions));
        }

        return result;
    }

    private static UserDto MapToUserDto(Models.Entities.User user, List<string> permissions)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Permissions = permissions,
            Roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList()
        };
    }
}
