using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Data.Seed;
using BlazorApp1.Infrastructure.Authentication;
using BlazorApp1.Infrastructure.Authorization;
using BlazorApp1.Services.Implementations;
using BlazorApp1.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("view_dashboard", policy =>
        policy.Requirements.Add(new PermissionRequirement("view_dashboard")));
    options.AddPolicy("view_profile", policy =>
        policy.Requirements.Add(new PermissionRequirement("view_profile")));
    options.AddPolicy("manage_users", policy =>
        policy.Requirements.Add(new PermissionRequirement("manage_users")));
    options.AddPolicy("view_reports", policy =>
        policy.Requirements.Add(new PermissionRequirement("view_reports")));
});

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionHandler>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("BlazorApp1Db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
