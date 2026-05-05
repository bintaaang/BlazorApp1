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

        // Seed Menu
        if (!context.Menus.Any())
        {
            var menus = new[]
            {
                new Menu
                {
                    Name = "Dashboard",
                    Url = "/dashboard",
                    Icon = "bi bi-speedometer2",
                    Order = 1,
                    PermissionName = "view_dashboard",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Profile",
                    Url = "/profile",
                    Icon = "bi bi-person",
                    Order = 2,
                    PermissionName = "view_profile",
                    IsActive = true
                },
                new Menu
                {
                    Name = "User Management",
                    Url = "/admin/users",
                    Icon = "bi bi-people",
                    Order = 3,
                    PermissionName = "manage_users",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Reports",
                    Url = "/admin/reports",
                    Icon = "bi bi-bar-chart",
                    Order = 4,
                    PermissionName = "view_reports",
                    IsActive = true
                }
            };

            context.Menus.AddRange(menus);
            await context.SaveChangesAsync();
        }

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
}
