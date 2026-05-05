using BlazorApp1.Data;
using BlazorApp1.Models.DTOs;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;

    public UserService(AppDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users
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
        var user = await _context.Users
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
        var users = await _context.Users
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
