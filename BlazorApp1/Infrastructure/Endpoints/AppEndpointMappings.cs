using BlazorApp1.Models.DTOs;
using BlazorApp1.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BlazorApp1.Infrastructure.Endpoints;

public static class AppEndpointMappings
{
    public static IEndpointRouteBuilder MapAppEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRootRedirect();
        endpoints.MapAuthEndpoints();

        return endpoints;
    }

    private static void MapRootRedirect(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext httpContext) =>
        {
            var target = httpContext.User.Identity?.IsAuthenticated == true ? "/dashboard" : "/login";
            return Results.Redirect(target);
        });
    }

    private static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", async (HttpContext httpContext, IAuthService authService) =>
        {
            var form = await httpContext.Request.ReadFormAsync();
            var request = new LoginRequest
            {
                Username = form["Username"].ToString(),
                Password = form["Password"].ToString()
            };

            var response = await authService.LoginAsync(request);
            if (!response.Success || response.User is null)
            {
                var message = Uri.EscapeDataString(response.Message ?? "Login gagal");
                return Results.Redirect($"/login?error={message}");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                new(ClaimTypes.Name, response.User.Username),
                new(ClaimTypes.Email, response.User.Email),
                new("FullName", response.User.FullName)
            };

            claims.AddRange(response.User.Permissions.Select(permission => new Claim("Permission", permission)));
            claims.AddRange(response.User.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Results.Redirect("/dashboard");
        }).DisableAntiforgery();

        endpoints.MapGet("/auth/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        });
    }
}
