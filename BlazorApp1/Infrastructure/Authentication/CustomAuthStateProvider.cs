using BlazorApp1.Models.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorApp1.Infrastructure.Authentication;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private UserDto? _currentUser;
    private bool _isAuthenticated = false;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_isAuthenticated && _currentUser != null)
        {
            var identity = CreateClaimsIdentity();
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        var anonymousIdentity = new ClaimsIdentity();
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymousIdentity)));
    }

    public async Task LoginAsync(UserDto user)
    {
        _currentUser = user;
        _isAuthenticated = true;

        var identity = CreateClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        await Task.CompletedTask;
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _isAuthenticated = false;

        var anonymousIdentity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(anonymousIdentity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        await Task.CompletedTask;
    }

    private ClaimsIdentity CreateClaimsIdentity()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _currentUser!.Id.ToString()),
            new Claim(ClaimTypes.Name, _currentUser.Username),
            new Claim(ClaimTypes.Email, _currentUser.Email),
            new Claim("FullName", _currentUser.FullName)
        };

        // Add permission claims
        foreach (var permission in _currentUser.Permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        // Add role claims
        foreach (var role in _currentUser.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsIdentity(claims, "Custom");
    }
}
