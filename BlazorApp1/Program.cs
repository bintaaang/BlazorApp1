using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Infrastructure.Authorization;
using BlazorApp1.Infrastructure.Database;
using BlazorApp1.Infrastructure.Endpoints;
using BlazorApp1.Infrastructure.Middleware;
using BlazorApp1.Services.Implementations;
using BlazorApp1.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var appRunId = Guid.NewGuid().ToString("N");
const string authCookiePrefix = ".BlazorApp1.Auth.";
var currentAuthCookieName = $"{authCookiePrefix}{appRunId}";
var builder = WebApplication.CreateBuilder(args);
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

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
    options.UseNpgsql(defaultConnection));
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("view_dashboard", policy => policy.Requirements.Add(new PermissionRequirement("view_dashboard")))
    .AddPolicy("view_profile", policy => policy.Requirements.Add(new PermissionRequirement("view_profile")))
    .AddPolicy("manage_users", policy => policy.Requirements.Add(new PermissionRequirement("manage_users")))
    .AddPolicy("view_reports", policy => policy.Requirements.Add(new PermissionRequirement("view_reports")));

var app = builder.Build();
await app.InitializeDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseStaticWebAssetPathRewrite();
app.UseAuthCookieCleanup(authCookiePrefix, currentAuthCookieName);
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapAppEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
