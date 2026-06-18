using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Infrastructure.Authorization;
using BlazorApp1.Infrastructure.Endpoints;
using BlazorApp1.Infrastructure.Middleware;
using BlazorApp1.Services.Administration.Permission;
using BlazorApp1.Services.Administration.UserManagement;
using BlazorApp1.Services.ApplicationMenu;
using BlazorApp1.Services.Auth;
using BlazorApp1.Services.MasterData.CustomerData;
using BlazorApp1.Services.Saas;
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
builder.Services.AddScoped<ICustomerDataService, CustomerDataService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ISaasDashboardService, SaasDashboardService>();
builder.Services.AddScoped<IOperationsReadService, OperationsReadService>();
builder.Services.AddScoped<ISaasManagementService, SaasManagementService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("view_dashboard", policy => policy.Requirements.Add(new PermissionRequirement("view_dashboard")))
    .AddPolicy("manage_users", policy => policy.Requirements.Add(new PermissionRequirement("manage_users")))
    .AddPolicy("view_reports", policy => policy.Requirements.Add(new PermissionRequirement("view_reports")))
    .AddPolicy("view_customer", policy => policy.Requirements.Add(new PermissionRequirement("view_customer")))
    .AddPolicy("view_saas", policy => policy.Requirements.Add(new PermissionRequirement("view_saas")))
    .AddPolicy("manage_tenants", policy => policy.Requirements.Add(new PermissionRequirement("manage_tenants")))
    .AddPolicy("manage_subscription_plans", policy => policy.Requirements.Add(new PermissionRequirement("manage_subscription_plans")))
    .AddPolicy("manage_service_items", policy => policy.Requirements.Add(new PermissionRequirement("manage_service_items")))
    .AddPolicy("view_work_orders", policy => policy.Requirements.Add(new PermissionRequirement("view_work_orders")))
    .AddPolicy("manage_work_orders", policy => policy.Requirements.Add(new PermissionRequirement("manage_work_orders")))
    .AddPolicy("view_invoices", policy => policy.Requirements.Add(new PermissionRequirement("view_invoices")))
    .AddPolicy("manage_invoices", policy => policy.Requirements.Add(new PermissionRequirement("manage_invoices")));

var app = builder.Build();

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
