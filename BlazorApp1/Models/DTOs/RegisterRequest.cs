using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konfirmasi password wajib diisi")]
    [Compare(nameof(Password), ErrorMessage = "Konfirmasi password tidak cocok")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    public string FullName { get; set; } = string.Empty;
}
