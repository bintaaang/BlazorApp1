using BootstrapBlazor.Components;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models;

public class User : BaseEntity, IValidatableObject
{
    public int? TenantId { get; set; }

    [Display(Name = "Username")]
    [PlaceHolder("Masukkan username")]
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [PlaceHolder("Masukkan email")]
    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [Display(Name = "Nama Lengkap")]
    [PlaceHolder("Masukkan nama lengkap")]
    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [PlaceHolder("Pilih status")]
    public bool IsActive { get; set; } = true;
    public int? CustomerDataId { get; set; }
    public Tenant? Tenant { get; set; }
    public CustomerData? CustomerData { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<UserPermission> UserPermissions { get; set; } = [];

    [NotMapped]
    public string CustomerName { get; set; } = string.Empty;

    [NotMapped]
    public string CustomerType { get; set; } = string.Empty;

    [NotMapped]
    public List<string> Permissions { get; set; } = [];

    [NotMapped]
    public List<string> MenuPermissions { get; set; } = [];

    [NotMapped]
    public List<string> Roles { get; set; } = [];

    [NotMapped]
    [Display(Name = "Role")]
    [PlaceHolder("Pilih role")]
    [Required(ErrorMessage = "Role wajib dipilih")]
    public string RoleName { get; set; } = "User";

    [NotMapped]
    [Display(Name = "Password")]
    [PlaceHolder("Masukkan password")]
    public string Password { get; set; } = string.Empty;

    [NotMapped]
    [Display(Name = "Konfirmasi Password")]
    [PlaceHolder("Konfirmasi password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [NotMapped]
    public bool IsEditMode { get; set; }

    [NotMapped]
    public string StatusText { get; set; } = string.Empty;

    [NotMapped]
    public string RolesText { get; set; } = string.Empty;

    [NotMapped]
    public int PermissionCount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult("Password wajib diisi", [nameof(Password)]);
        }

        if (!string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult("Konfirmasi password tidak cocok", [nameof(ConfirmPassword)]);
            }
        }
    }
}
