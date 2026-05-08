using BlazorApp1.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Seed Roles
        if (!context.Roles.Any())
        {
            var admin = new Role { Name = "Admin", Description = "Administrator" };
            var user = new Role { Name = "User", Description = "Regular User" };

            context.Roles.AddRange(admin, user);
            await context.SaveChangesAsync();
        }

        // Seed Permissions
        if (!context.Permissions.Any())
        {
            var permissions = new[]
            {
                new Permission { Name = "view_dashboard", Description = "View Dashboard" },
                new Permission { Name = "view_profile", Description = "View Profile" },
                new Permission { Name = "manage_users", Description = "Manage Users" },
                new Permission { Name = "view_reports", Description = "View Reports" }
            };

            context.Permissions.AddRange(permissions);
            await context.SaveChangesAsync();
        }

        // Seed RolePermissions
        if (!context.RolePermissions.Any())
        {
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            var userRole = context.Roles.First(r => r.Name == "User");
            var permissions = context.Permissions.ToList();

            // Admin gets all permissions
            foreach (var permission in permissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            // User gets limited permissions
            var userPermissions = permissions.Where(p =>
                p.Name == "view_dashboard" || p.Name == "view_profile").ToList();

            foreach (var permission in userPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = permission.Id
                });
            }

            await context.SaveChangesAsync();
        }

        // Seed Modules
        await EnsureModuleAsync(context, "Main", "Main application menu", "bi bi-grid", 1);
        await EnsureModuleAsync(context, "Administration", "Administration menu", "bi bi-shield-lock", 2);

        // Seed Menu
        if (!context.Menus.Any())
        {
            var mainModuleId = context.Modules.First(m => m.Name == "Main").Id;
            var adminModuleId = context.Modules.First(m => m.Name == "Administration").Id;

            var menus = new[]
            {
                new Menu
                {
                    Name = "Dashboard",
                    Url = "/dashboard",
                    Icon = "bi bi-speedometer2",
                    ModuleId = mainModuleId,
                    Order = 1,
                    PermissionName = "view_dashboard",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Profile",
                    Url = "/profile",
                    Icon = "bi bi-person",
                    ModuleId = mainModuleId,
                    Order = 2,
                    PermissionName = "view_profile",
                    IsActive = true
                },
                new Menu
                {
                    Name = "User Management",
                    Url = "/admin/users",
                    Icon = "bi bi-people",
                    ModuleId = adminModuleId,
                    Order = 3,
                    PermissionName = "manage_users",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Reports",
                    Url = "/admin/reports",
                    Icon = "bi bi-bar-chart",
                    ModuleId = adminModuleId,
                    Order = 4,
                    PermissionName = "view_reports",
                    IsActive = true
                }
            };

            context.Menus.AddRange(menus);
            await context.SaveChangesAsync();
        }

        await SyncMenuModulesAsync(context);

        // Seed Users (Demo)
        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            var userRole = context.Roles.First(r => r.Name == "User");

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                FullName = "Administrator",
                PasswordHash = HashPassword("admin123"),
                IsActive = true
            };

            var regularUser = new User
            {
                Username = "user",
                Email = "user@example.com",
                FullName = "Regular User",
                PasswordHash = HashPassword("user123"),
                IsActive = true
            };

            context.Users.AddRange(adminUser, regularUser);
            await context.SaveChangesAsync();

            // Assign roles to users
            var adminUserDb = context.Users.First(u => u.Username == "admin");
            var userDb = context.Users.First(u => u.Username == "user");

            context.UserRoles.AddRange(
                new UserRole { UserId = adminUserDb.Id, RoleId = adminRole.Id },
                new UserRole { UserId = userDb.Id, RoleId = userRole.Id }
            );

            await context.SaveChangesAsync();
        }

    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static async Task EnsureModuleAsync(
        AppDbContext context,
        string name,
        string description,
        string icon,
        int order)
    {
        var module = context.Modules.FirstOrDefault(m => m.Name == name);
        if (module == null)
        {
            context.Modules.Add(new Module
            {
                Name = name,
                Description = description,
                Icon = icon,
                Order = order,
                IsActive = true
            });
        }
        else
        {
            module.Description = description;
            module.Icon = icon;
            module.Order = order;
            module.IsActive = true;
        }

        await context.SaveChangesAsync();
    }

    private static async Task SyncMenuModulesAsync(AppDbContext context)
    {
        var mainModuleId = context.Modules.First(m => m.Name == "Main").Id;
        var adminModuleId = context.Modules.First(m => m.Name == "Administration").Id;

        var moduleByUrl = new Dictionary<string, int>
        {
            ["/dashboard"] = mainModuleId,
            ["/profile"] = mainModuleId,
            ["/admin/users"] = adminModuleId,
            ["/admin/reports"] = adminModuleId
        };

        foreach (var menu in context.Menus)
        {
            if (moduleByUrl.TryGetValue(menu.Url, out var moduleId))
            {
                menu.ModuleId = moduleId;
            }
        }

        await context.SaveChangesAsync();
    }
}
