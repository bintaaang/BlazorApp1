# BlazorApp1

BlazorApp1 adalah aplikasi web berbasis Blazor Server untuk contoh portfolio admin dashboard dengan autentikasi cookie, RBAC (Role-Based Access Control), menu dinamis berbasis permission, user management, dan master data customer.

Project ini dibuat dengan pendekatan layered sederhana agar mudah dibaca dan dikembangkan. UI berada di `Components`, business/service logic berada di `Services`, konfigurasi database berada di `Data`, dan kebutuhan teknis aplikasi seperti authorization, endpoint, middleware, serta database initializer berada di `Infrastructure`.

## Tech Stack

- .NET 9
- Blazor Server Interactive
- Entity Framework Core
- PostgreSQL
- BootstrapBlazor
- Cookie Authentication
- Policy-based Authorization

## Fitur Utama

- Login dan logout menggunakan cookie authentication.
- RBAC menggunakan role, permission, dan policy authorization.
- Menu sidebar dinamis berdasarkan permission user.
- User management untuk membuat, mengubah, dan menghapus user.
- Assign role dan menu access per user.
- Master data customer.
- Audit field dasar: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`.
- Auto database initialization saat aplikasi dijalankan.

## Struktur Project

```text
BlazorApp1/
+-- Components/
|   +-- Layout/                  # Layout utama, empty layout, reconnect modal
|   +-- Pages/                   # Halaman aplikasi: Auth, Dashboard, Admin, Master Data
+-- Data/
|   +-- AppDbContext.cs          # EF Core DbContext dan mapping relasi
|   +-- Seed/DbSeeder.cs         # Seed role, permission, menu, dan user demo
+-- Infrastructure/
|   +-- Authorization/           # PermissionRequirement dan PermissionHandler
|   +-- Database/                # Database initializer dan schema helper
|   +-- Endpoints/               # Endpoint login/logout
|   +-- Middleware/              # Middleware aplikasi
|   +-- Table/                   # Helper query table
+-- Models/
|   +-- Administration/          # Model user, role, permission
|   +-- ApplicationMenu/         # Model module dan menu
|   +-- Auth/                    # Model request/response login
|   +-- Common/                  # BaseEntity dan AuditableEntity
|   +-- MasterData/              # Model customer data
+-- Services/
|   +-- Administration/
|   |   +-- Permission/          # IPermissionService dan PermissionService
|   |   +-- UserManagement/      # IUserService dan UserService
|   +-- ApplicationMenu/         # IMenuService dan MenuService
|   +-- Auth/                    # IAuthService dan AuthService
|   +-- MasterData/CustomerData/ # ICustomerDataService dan CustomerDataService
+-- wwwroot/sql/                 # Script SQL tambahan untuk menu master data
```

## Konfigurasi

File konfigurasi lokal tidak disimpan ke git. Gunakan file example sebagai template:

```bash
copy BlazorApp1\appsettings.example.json BlazorApp1\appsettings.json
```

Lalu sesuaikan connection string PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=BlazorApp1Db;Username=your_username;Password=your_password;Include Error Detail=true"
  }
}
```

## Cara Menjalankan

1. Pastikan PostgreSQL sudah berjalan.
2. Buat database dan user sesuai connection string.

```sql
CREATE DATABASE "BlazorApp1Db";
```

3. Restore dependency dan jalankan aplikasi.

```bash
dotnet restore BlazorApp1.slnx
dotnet run --project BlazorApp1\BlazorApp1.csproj
```

4. Buka URL yang muncul di terminal, biasanya `https://localhost:xxxx` atau `http://localhost:xxxx`.

Saat pertama kali aplikasi berjalan, database akan dibuat otomatis melalui `EnsureCreatedAsync`, schema tambahan akan disiapkan oleh `AppDatabaseInitializer`, lalu data awal akan dibuat oleh `DbSeeder`.

## Akun Demo

| Role | Username | Password |
| --- | --- | --- |
| Admin | `admin` | `admin123` |
| User | `user` | `user123` |

## Relasi Database

Database utama menggunakan tabel berikut:

| Tabel | Fungsi |
| --- | --- |
| `AppUsers` | Data user aplikasi |
| `AppRoles` | Data role seperti Admin dan User |
| `AppPermissions` | Data permission, contoh `view_dashboard` |
| `AppUserRoles` | Relasi many-to-many user dan role |
| `AppRolePermissions` | Relasi many-to-many role dan permission |
| `AppUserPermissions` | Permission tambahan langsung ke user |
| `AppModules` | Group menu aplikasi |
| `AppMenus` | Menu aplikasi yang diikat ke permission |
| `CustomerData` | Master data customer |

Relasi penting:

- `AppUsers` many-to-many `AppRoles` melalui `AppUserRoles`.
- `AppRoles` many-to-many `AppPermissions` melalui `AppRolePermissions`.
- `AppUsers` many-to-many `AppPermissions` melalui `AppUserPermissions`.
- `AppModules` one-to-many `AppMenus`.
- `AppMenus` dapat memiliki parent menu melalui `ParentId`.
- `CustomerData` one-to-many `AppUsers`; user dapat memiliki satu customer, customer dapat dimiliki banyak user.

## Cara Kerja RBAC

RBAC di project ini memakai kombinasi role, permission, policy, dan menu.

1. User login melalui `/auth/login`.
2. `AuthService` validasi username dan password.
3. Permission user diambil dari `PermissionService`.
4. Permission dimasukkan ke claims dengan tipe `permission`.
5. Halaman dilindungi dengan attribute seperti:

```csharp
@attribute [Authorize(Policy = "manage_users")]
```

6. `PermissionHandler` mengecek apakah claim permission user memenuhi policy.
7. `MenuService` hanya menampilkan menu yang permission-nya dimiliki user.

## Cara Menambah RBAC Baru

Contoh menambahkan menu dan permission baru bernama `view_customer`.

### 1. Tambahkan Policy

Daftarkan policy di `Program.cs`:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("view_customer", policy => policy.Requirements.Add(new PermissionRequirement("view_customer")));
```

### 2. Lindungi Halaman

Tambahkan attribute pada halaman Razor:

```csharp
@attribute [Authorize(Policy = "view_customer")]
```

### 3. Tambahkan Permission

Tambahkan permission ke tabel `AppPermissions`:

```sql
INSERT INTO "AppPermissions" ("Name", "Description", "CreatedAt", "CreatedBy")
VALUES ('view_customer', 'Can view customer menu', NOW(), 'admin');
```

### 4. Assign Permission ke Role

```sql
INSERT INTO "AppRolePermissions" ("RoleId", "PermissionId", "CreatedAt", "CreatedBy")
SELECT r."Id", p."Id", NOW(), 'admin'
FROM "AppRoles" r
JOIN "AppPermissions" p ON p."Name" = 'view_customer'
WHERE r."Name" = 'Admin';
```

### 5. Tambahkan Menu

```sql
INSERT INTO "AppMenus" (
    "Name", "Url", "Icon", "ParentId", "ModuleId", "Order",
    "PermissionName", "IsActive", "CreatedAt", "CreatedBy"
)
SELECT
    'Customer', '/master/customer', 'bi bi-people', NULL, m."Id", 10,
    'view_customer', TRUE, NOW(), 'admin'
FROM "AppModules" m
WHERE m."Name" = 'Master Data';
```

Script lengkap untuk contoh menu Customer tersedia di:

```text
BlazorApp1/wwwroot/sql/add-master-data-customer-menu.sql
```

## Catatan Arsitektur

Project ini memakai layered architecture sederhana:

- `Components` fokus pada UI.
- `Services` fokus pada use case dan business flow.
- `Models` menyimpan entity dan DTO sederhana.
- `Data` menyimpan EF Core DbContext dan seed.
- `Infrastructure` menyimpan detail teknis aplikasi.

Ini belum dipisah menjadi Clean Architecture multi-project penuh, tetapi struktur sudah dibuat konsisten agar mudah dinaikkan ke pendekatan `Domain`, `Application`, `Infrastructure`, dan `Web` jika project berkembang.

## Git Notes

File konfigurasi lokal seperti `appsettings.json` dan `appsettings.Development.json` di-ignore agar connection string dan password database tidak ikut ter-push ke GitHub. Gunakan `appsettings.example.json` sebagai template konfigurasi.
