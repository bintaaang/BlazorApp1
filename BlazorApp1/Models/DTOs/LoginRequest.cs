using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    public string Password { get; set; } = string.Empty;
}
