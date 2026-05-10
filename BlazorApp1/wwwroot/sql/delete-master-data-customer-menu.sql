BEGIN;

-- 1. Hapus menu Customer
DELETE FROM "AppMenus"
WHERE "Url" = '/master/customer'
  AND "PermissionName" = 'view_customer';

-- 2. Hapus permission view_customer dari role Admin
DELETE FROM "AppRolePermissions" rp
USING "AppRoles" r, "AppPermissions" p
WHERE rp."RoleId" = r."Id"
  AND rp."PermissionId" = p."Id"
  AND r."Name" = 'Admin'
  AND p."Name" = 'view_customer';

-- 3. Hapus permission jika sudah tidak dipakai role/menu lain
DELETE FROM "AppPermissions" p
WHERE p."Name" = 'view_customer'
  AND NOT EXISTS (
      SELECT 1
      FROM "AppRolePermissions" rp
      WHERE rp."PermissionId" = p."Id"
  )
  AND NOT EXISTS (
      SELECT 1
      FROM "AppMenus" m
      WHERE m."PermissionName" = p."Name"
  );

-- 4. Hapus module Master Data jika sudah tidak punya menu
DELETE FROM "AppModules" m
WHERE m."Name" = 'Master Data'
  AND NOT EXISTS (
      SELECT 1
      FROM "AppMenus" menu
      WHERE menu."ModuleId" = m."Id"
  );

COMMIT;
