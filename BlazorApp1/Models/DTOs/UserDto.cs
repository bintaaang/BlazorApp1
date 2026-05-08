using BootstrapBlazor.Components;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models.DTOs;

public class UserDto : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Username")]
    [PlaceHolder("Masukkan username")]
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [PlaceHolder("Masukkan email")]
    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Nama Lengkap")]
    [PlaceHolder("Masukkan nama lengkap")]
    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [PlaceHolder("Pilih status")]
    public bool IsActive { get; set; }

    public List<string> Permissions { get; set; } = [];
    public List<string> Roles { get; set; } = [];

    [Display(Name = "Role")]
    [PlaceHolder("Pilih role")]
    [Required(ErrorMessage = "Role wajib dipilih")]
    public string RoleName { get; set; } = "User";

    [Display(Name = "Password")]
    [PlaceHolder("Masukkan password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Konfirmasi Password")]
    [PlaceHolder("Konfirmasi password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsEditMode { get; set; }

    public string StatusText { get; set; } = string.Empty;
    public string RolesText { get; set; } = string.Empty;
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
