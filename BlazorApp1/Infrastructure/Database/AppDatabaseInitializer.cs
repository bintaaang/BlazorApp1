using BlazorApp1.Data;
using BlazorApp1.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Infrastructure.Database;

public static class AppDatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await using var dbContext = await scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<AppDbContext>>()
            .CreateDbContextAsync();

        await dbContext.Database.EnsureCreatedAsync();
        await EnsureModuleSchemaAsync(dbContext);
        await EnsureUserPermissionSchemaAsync(dbContext);
        await EnsureCustomerDataSchemaAsync(dbContext);
        await DbSeeder.SeedAsync(dbContext);
    }

    private static async Task EnsureModuleSchemaAsync(AppDbContext dbContext)
    {
        // Ensure AppModules table exists (already created by EnsureCreatedAsync)
        // Insert default module if empty
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [AppModules])
            INSERT INTO [AppModules] ([Name], [Description], [Icon], [Order], [IsActive], [CreatedAt], [CreatedBy])
            VALUES ('General', 'Default application module', 'bi bi-grid', 1, 1, GETUTCDATE(), 'system');
            """);

        // Add ModuleId column to AppMenus if not exists
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('[AppMenus]', 'ModuleId') IS NULL
            BEGIN
                ALTER TABLE [AppMenus] ADD [ModuleId] int NOT NULL DEFAULT 1;
            END
            """);

        // Add foreign key if not exists
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_AppMenus_AppModules_ModuleId'
            )
            BEGIN
                ALTER TABLE [AppMenus]
                ADD CONSTRAINT [FK_AppMenus_AppModules_ModuleId]
                FOREIGN KEY ([ModuleId]) REFERENCES [AppModules] ([Id])
                ON DELETE NO ACTION;
            END
            """);
    }

    private static async Task EnsureUserPermissionSchemaAsync(AppDbContext dbContext)
    {
        // AppUserPermissions table is already created by EnsureCreatedAsync
        // Add foreign keys if they don't exist
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_AppUserPermissions_AppUsers_UserId'
            )
            BEGIN
                ALTER TABLE [AppUserPermissions]
                ADD CONSTRAINT [FK_AppUserPermissions_AppUsers_UserId]
                FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([Id])
                ON DELETE CASCADE;
            END
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_AppUserPermissions_AppPermissions_PermissionId'
            )
            BEGIN
                ALTER TABLE [AppUserPermissions]
                ADD CONSTRAINT [FK_AppUserPermissions_AppPermissions_PermissionId]
                FOREIGN KEY ([PermissionId]) REFERENCES [AppPermissions] ([Id])
                ON DELETE CASCADE;
            END
            """);
    }

    private static async Task EnsureCustomerDataSchemaAsync(AppDbContext dbContext)
    {
        // Add CustomerDataId column to AppUsers if not exists
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('[AppUsers]', 'CustomerDataId') IS NULL
            BEGIN
                ALTER TABLE [AppUsers] ADD [CustomerDataId] int NULL;
            END
            """);

        // Drop old FK if exists (migration cleanup)
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_CustomerData_AppUsers_UserId'
            )
            BEGIN
                ALTER TABLE [CustomerData] DROP CONSTRAINT [FK_CustomerData_AppUsers_UserId];
            END
            """);

        // Drop old UserId column from CustomerData if exists (migration cleanup)
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('[CustomerData]', 'UserId') IS NOT NULL
            BEGIN
                ALTER TABLE [CustomerData] DROP COLUMN [UserId];
            END
            """);

        // Add TenantId column to CustomerData if not exists
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('[CustomerData]', 'TenantId') IS NULL
            BEGIN
                ALTER TABLE [CustomerData] ADD [TenantId] int NULL;
            END
            """);

        // Add TenantId column to AppUsers if not exists
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('[AppUsers]', 'TenantId') IS NULL
            BEGIN
                ALTER TABLE [AppUsers] ADD [TenantId] int NULL;
            END
            """);
        // Add FK AppUsers -> CustomerData
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_AppUsers_CustomerData_CustomerDataId'
            )
            BEGIN
                ALTER TABLE [AppUsers]
                ADD CONSTRAINT [FK_AppUsers_CustomerData_CustomerDataId]
                FOREIGN KEY ([CustomerDataId]) REFERENCES [CustomerData] ([Id])
                ON DELETE SET NULL;
            END
            """);

        // Create indexes
        await dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes 
                WHERE name = 'IX_CustomerData_CustomerName'
            )
            CREATE INDEX [IX_CustomerData_CustomerName]
            ON [CustomerData] ([CustomerName]);
            """);

        await dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes 
                WHERE name = 'IX_AppUsers_CustomerDataId'
            )
            CREATE INDEX [IX_AppUsers_CustomerDataId]
            ON [AppUsers] ([CustomerDataId]);
            """);
    }
}
