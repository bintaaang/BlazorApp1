using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Data.Seed;
using BlazorApp1.Infrastructure.Authorization;
using BlazorApp1.Models.DTOs;
using BlazorApp1.Services.Implementations;
using BlazorApp1.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var appRunId = Guid.NewGuid().ToString("N");
const string authCookiePrefix = ".BlazorApp1.Auth.";
var currentAuthCookieName = $"{authCookiePrefix}{appRunId}";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBootstrapBlazor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = currentAuthCookieName;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("view_dashboard", policy => policy.Requirements.Add(new PermissionRequirement("view_dashboard")))
    .AddPolicy("view_profile", policy => policy.Requirements.Add(new PermissionRequirement("view_profile")))
    .AddPolicy("manage_users", policy => policy.Requirements.Add(new PermissionRequirement("manage_users")))
    .AddPolicy("view_reports", policy => policy.Requirements.Add(new PermissionRequirement("view_reports")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await using var dbContext = await scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<AppDbContext>>()
        .CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.Use(async (httpContext, next) =>
{
    var staleCookieNames = httpContext.Request.Cookies.Keys
        .Where(cookieName =>
            cookieName.StartsWith(authCookiePrefix, StringComparison.Ordinal) &&
            !string.Equals(cookieName, currentAuthCookieName, StringComparison.Ordinal))
        .ToList();

    foreach (var staleCookieName in staleCookieNames)
    {
        httpContext.Response.Cookies.Delete(staleCookieName, new CookieOptions
        {
            Path = "/"
        });
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/auth/login", async (HttpContext httpContext, IAuthService authService) =>
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
        new Claim(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
        new Claim(ClaimTypes.Name, response.User.Username),
        new Claim(ClaimTypes.Email, response.User.Email),
        new Claim("FullName", response.User.FullName)
    };

    claims.AddRange(response.User.Permissions.Select(permission => new Claim("Permission", permission)));
    claims.AddRange(response.User.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Redirect("/dashboard");
}).DisableAntiforgery();

app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
