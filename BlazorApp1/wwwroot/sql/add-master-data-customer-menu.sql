BEGIN;

-- 1. Tambah module Master Data
INSERT INTO "AppModules" ("Name", "Description", "Icon", "Order", "IsActive", "CreatedAt", "CreatedBy")
SELECT 'Master Data', 'Master data module', 'bi bi-database', 20, TRUE, NOW(), 'admin'
WHERE NOT EXISTS (
    SELECT 1 FROM "AppModules" WHERE "Name" = 'Master Data'
);

-- 2. Tambah permission untuk menu Customer
INSERT INTO "AppPermissions" ("Name", "Description", "CreatedAt", "CreatedBy")
SELECT 'view_customer', 'Can view customer menu', NOW(), 'admin'
WHERE NOT EXISTS (
    SELECT 1 FROM "AppPermissions" WHERE "Name" = 'view_customer'
);

-- 3. Berikan permission view_customer ke role Admin
INSERT INTO "AppRolePermissions" ("RoleId", "PermissionId", "CreatedAt", "CreatedBy")
SELECT r."Id", p."Id", NOW(), 'admin'
FROM "AppRoles" r
JOIN "AppPermissions" p ON p."Name" = 'view_customer'
WHERE r."Name" = 'Admin'
AND NOT EXISTS (
    SELECT 1
    FROM "AppRolePermissions" rp
    WHERE rp."RoleId" = r."Id"
      AND rp."PermissionId" = p."Id"
);

-- 4. Tambah menu Customer di module Master Data
INSERT INTO "AppMenus" (
    "Name",
    "Url",
    "Icon",
    "ParentId",
    "ModuleId",
    "Order",
    "PermissionName",
    "IsActive",
    "CreatedAt",
    "CreatedBy"
)
SELECT
    'Customer',
    '/master/customer',
    'bi bi-people',
    NULL,
    m."Id",
    10,
    'view_customer',
    TRUE,
    NOW(),
    'admin'
FROM "AppModules" m
WHERE m."Name" = 'Master Data'
AND NOT EXISTS (
    SELECT 1 FROM "AppMenus" WHERE "Url" = '/master/customer'
);

COMMIT;
