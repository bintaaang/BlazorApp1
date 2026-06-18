using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<CustomerData> CustomerData => Set<CustomerData>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("AppUsers");
        modelBuilder.Entity<Role>().ToTable("AppRoles");
        modelBuilder.Entity<Permission>().ToTable("AppPermissions");
        modelBuilder.Entity<Module>().ToTable("AppModules");
        modelBuilder.Entity<Menu>().ToTable("AppMenus");
        modelBuilder.Entity<CustomerData>().ToTable("CustomerData");
        modelBuilder.Entity<Tenant>().ToTable("SaasTenants");
        modelBuilder.Entity<SubscriptionPlan>().ToTable("SaasSubscriptionPlans");
        modelBuilder.Entity<TenantSubscription>().ToTable("SaasTenantSubscriptions");
        modelBuilder.Entity<ServiceItem>().ToTable("ServiceItems");
        modelBuilder.Entity<WorkOrder>().ToTable("WorkOrders");
        modelBuilder.Entity<Invoice>().ToTable("Invoices");
        modelBuilder.Entity<Payment>().ToTable("Payments");
        modelBuilder.Entity<UserRole>().ToTable("AppUserRoles");
        modelBuilder.Entity<RolePermission>().ToTable("AppRolePermissions");
        modelBuilder.Entity<UserPermission>().ToTable("AppUserPermissions");

        // RolePermission composite key
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPermission>()
            .HasKey(up => new { up.UserId, up.PermissionId });

        modelBuilder.Entity<UserPermission>()
            .HasOne(up => up.User)
            .WithMany(u => u.UserPermissions)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPermission>()
            .HasOne(up => up.Permission)
            .WithMany(p => p.UserPermissions)
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserRole composite key
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Menu>()
            .HasOne(m => m.Module)
            .WithMany(m => m.Menus)
            .HasForeignKey(m => m.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Menu>()
            .HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(user => user.CustomerData)
            .WithMany(customer => customer.Users)
            .HasForeignKey(user => user.CustomerDataId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(user => user.Tenant)
            .WithMany(tenant => tenant.Users)
            .HasForeignKey(user => user.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CustomerData>()
            .HasOne(customer => customer.Tenant)
            .WithMany(tenant => tenant.Customers)
            .HasForeignKey(customer => customer.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TenantSubscription>()
            .HasOne(subscription => subscription.Tenant)
            .WithMany(tenant => tenant.Subscriptions)
            .HasForeignKey(subscription => subscription.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantSubscription>()
            .HasOne(subscription => subscription.SubscriptionPlan)
            .WithMany(plan => plan.Subscriptions)
            .HasForeignKey(subscription => subscription.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceItem>()
            .HasOne(item => item.Tenant)
            .WithMany(tenant => tenant.ServiceItems)
            .HasForeignKey(item => item.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.Tenant)
            .WithMany(tenant => tenant.WorkOrders)
            .HasForeignKey(order => order.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(order => order.CustomerData)
            .WithMany(customer => customer.WorkOrders)
            .HasForeignKey(order => order.CustomerDataId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Invoice>()
            .HasOne(invoice => invoice.Tenant)
            .WithMany(tenant => tenant.Invoices)
            .HasForeignKey(invoice => invoice.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Invoice>()
            .HasOne(invoice => invoice.CustomerData)
            .WithMany(customer => customer.Invoices)
            .HasForeignKey(invoice => invoice.CustomerDataId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Invoice>()
            .HasOne(invoice => invoice.WorkOrder)
            .WithMany(order => order.Invoices)
            .HasForeignKey(invoice => invoice.WorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Payment>()
            .HasOne(payment => payment.Invoice)
            .WithMany(invoice => invoice.Payments)
            .HasForeignKey(payment => payment.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<Menu>()
            .HasIndex(m => m.Url);

        modelBuilder.Entity<Module>()
            .HasIndex(m => m.Name)
            .IsUnique();

        modelBuilder.Entity<CustomerData>()
            .HasIndex(customer => customer.CustomerName);

        modelBuilder.Entity<Tenant>()
            .HasIndex(tenant => tenant.Slug)
            .IsUnique();

        modelBuilder.Entity<SubscriptionPlan>()
            .HasIndex(plan => plan.Name)
            .IsUnique();

        modelBuilder.Entity<ServiceItem>()
            .HasIndex(item => new { item.TenantId, item.Name });

        modelBuilder.Entity<WorkOrder>()
            .HasIndex(order => new { order.TenantId, order.OrderNumber })
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .HasIndex(invoice => new { invoice.TenantId, invoice.InvoiceNumber })
            .IsUnique();
    }

    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy ??= "system";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy ??= "system";
            }
        }
    }
}
