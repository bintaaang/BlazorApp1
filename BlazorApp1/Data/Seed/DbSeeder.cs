using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;
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
                new Permission { Name = "manage_users", Description = "Manage Users" },
                new Permission { Name = "view_reports", Description = "View Reports" },
                // SaaS
                new Permission { Name = "view_saas", Description = "View SaaS Overview" },
                new Permission { Name = "manage_tenants", Description = "Manage Tenants" },
                new Permission { Name = "manage_subscriptions", Description = "Manage Subscriptions" },
                // Operations
                new Permission { Name = "view_operations", Description = "View Operations" },
                new Permission { Name = "manage_serviceitems", Description = "Manage Service Items" },
                new Permission { Name = "manage_workorders", Description = "Manage Work Orders" },
                // Billing
                new Permission { Name = "view_billing", Description = "View Billing" },
                new Permission { Name = "manage_invoices", Description = "Manage Invoices" },
                new Permission { Name = "manage_payments", Description = "Manage Payments" }
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
            var userPermissions = permissions.Where(p => p.Name == "view_dashboard").ToList();

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

        // Seed Modules (always ensure these exist)
        await EnsureModuleAsync(context, "Main", "Main application menu", "bi bi-grid", 1);
        await EnsureModuleAsync(context, "Administration", "Administration menu", "bi bi-shield-lock", 2);
        await EnsureModuleAsync(context, "SaaS", "SaaS & Enterprise menu", "bi bi-cloud", 3);
        await EnsureModuleAsync(context, "Operations", "Operations menu", "bi bi-tools", 4);
        await EnsureModuleAsync(context, "Billing", "Billing & Finance menu", "bi bi-credit-card", 5);
        
        // Verify all modules exist before using them
        var mainModule = await context.Modules.FirstAsync(m => m.Name == "Main");
        var adminModule = await context.Modules.FirstAsync(m => m.Name == "Administration");
        var saasModule = await context.Modules.FirstAsync(m => m.Name == "SaaS");
        var opsModule = await context.Modules.FirstAsync(m => m.Name == "Operations");
        var billingModule = await context.Modules.FirstAsync(m => m.Name == "Billing");

        // Seed Menu
        if (!context.Menus.Any())
        {
            var mainModuleId = mainModule.Id;
            var adminModuleId = adminModule.Id;
            var saasModuleId = saasModule.Id;
            var opsModuleId = opsModule.Id;
            var billingModuleId = billingModule.Id;

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
                },
                // SaaS Menus
                new Menu
                {
                    Name = "SaaS Overview",
                    Url = "/saas",
                    Icon = "bi bi-cloud",
                    ModuleId = saasModuleId,
                    Order = 1,
                    PermissionName = "view_saas",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Tenants",
                    Url = "/saas/tenants",
                    Icon = "bi bi-building",
                    ModuleId = saasModuleId,
                    Order = 2,
                    PermissionName = "manage_tenants",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Subscription Plans",
                    Url = "/saas/plans",
                    Icon = "bi bi-card-list",
                    ModuleId = saasModuleId,
                    Order = 3,
                    PermissionName = "manage_subscriptions",
                    IsActive = true
                },
                // Operations Menus
                new Menu
                {
                    Name = "Service Items",
                    Url = "/operations/service-items",
                    Icon = "bi bi-box",
                    ModuleId = opsModuleId,
                    Order = 1,
                    PermissionName = "manage_serviceitems",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Work Orders",
                    Url = "/operations/work-orders",
                    Icon = "bi bi-clipboard-check",
                    ModuleId = opsModuleId,
                    Order = 2,
                    PermissionName = "manage_workorders",
                    IsActive = true
                },
                // Billing Menus
                new Menu
                {
                    Name = "Invoices",
                    Url = "/billing/invoices",
                    Icon = "bi bi-file-earmark-text",
                    ModuleId = billingModuleId,
                    Order = 1,
                    PermissionName = "manage_invoices",
                    IsActive = true
                },
                new Menu
                {
                    Name = "Payments",
                    Url = "/billing/payments",
                    Icon = "bi bi-credit-card",
                    ModuleId = billingModuleId,
                    Order = 2,
                    PermissionName = "manage_payments",
                    IsActive = true
                }
            };

            context.Menus.AddRange(menus);
            await context.SaveChangesAsync();
        }

        await SyncMenuModulesAsync(context);

        // Ensure new permissions exist (idempotent)
        var existingPermissionNames = context.Permissions.Select(p => p.Name).ToHashSet();
        var newPermissions = new[]
        {
            new Permission { Name = "view_saas", Description = "View SaaS Overview" },
            new Permission { Name = "manage_tenants", Description = "Manage Tenants" },
            new Permission { Name = "manage_subscriptions", Description = "Manage Subscriptions" },
            new Permission { Name = "view_operations", Description = "View Operations" },
            new Permission { Name = "manage_serviceitems", Description = "Manage Service Items" },
            new Permission { Name = "manage_workorders", Description = "Manage Work Orders" },
            new Permission { Name = "view_billing", Description = "View Billing" },
            new Permission { Name = "manage_invoices", Description = "Manage Invoices" },
            new Permission { Name = "manage_payments", Description = "Manage Payments" }
        };
        foreach (var p in newPermissions)
        {
            if (!existingPermissionNames.Contains(p.Name))
                context.Permissions.Add(p);
        }
        await context.SaveChangesAsync();

        // Menambahkan menu baru jika belum ada
        var existingMenuUrls = context.Menus.Select(m => m.Url).ToHashSet();
        
        // Pastikan Admin role punya semua permission baru
        var adminRoleEntity = await context.Roles.FirstAsync(r => r.Name == "Admin");
        var existingRolePermissions = context.RolePermissions
            .Where(rp => rp.RoleId == adminRoleEntity.Id)
            .Select(rp => rp.PermissionId)
            .ToHashSet();
        var allPermissions = await context.Permissions.ToListAsync();
        foreach (var perm in allPermissions)
        {
            if (!existingRolePermissions.Contains(perm.Id))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRoleEntity.Id,
                    PermissionId = perm.Id
                });
            }
        }
        await context.SaveChangesAsync();

        var newMenus = new[]
        {
            new Menu { Name = "SaaS Overview", Url = "/saas", Icon = "bi bi-cloud", ModuleId = saasModule.Id, Order = 1, PermissionName = "view_saas", IsActive = true },
            new Menu { Name = "Tenants", Url = "/saas/tenants", Icon = "bi bi-building", ModuleId = saasModule.Id, Order = 2, PermissionName = "manage_tenants", IsActive = true },
            new Menu { Name = "Subscription Plans", Url = "/saas/plans", Icon = "bi bi-card-list", ModuleId = saasModule.Id, Order = 3, PermissionName = "manage_subscriptions", IsActive = true },
            new Menu { Name = "Service Items", Url = "/operations/service-items", Icon = "bi bi-box", ModuleId = opsModule.Id, Order = 1, PermissionName = "manage_serviceitems", IsActive = true },
            new Menu { Name = "Work Orders", Url = "/operations/work-orders", Icon = "bi bi-clipboard-check", ModuleId = opsModule.Id, Order = 2, PermissionName = "manage_workorders", IsActive = true },
            new Menu { Name = "Invoices", Url = "/billing/invoices", Icon = "bi bi-file-earmark-text", ModuleId = billingModule.Id, Order = 1, PermissionName = "manage_invoices", IsActive = true }
        };
        foreach (var m in newMenus)
        {
            if (!existingMenuUrls.Contains(m.Url))
                context.Menus.Add(m);
        }
        await context.SaveChangesAsync();

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
        var saasModuleId = context.Modules.First(m => m.Name == "SaaS").Id;
        var opsModuleId = context.Modules.First(m => m.Name == "Operations").Id;
        var billingModuleId = context.Modules.First(m => m.Name == "Billing").Id;

        var moduleByUrl = new Dictionary<string, int>
        {
            ["/dashboard"] = mainModuleId,
            ["/admin/users"] = adminModuleId,
            ["/admin/reports"] = adminModuleId,
            ["/saas"] = saasModuleId,
            ["/saas/tenants"] = saasModuleId,
            ["/saas/plans"] = saasModuleId,
            ["/operations/service-items"] = opsModuleId,
            ["/operations/work-orders"] = opsModuleId,
            ["/billing/invoices"] = billingModuleId
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
