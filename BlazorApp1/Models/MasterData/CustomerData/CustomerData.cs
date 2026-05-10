using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models;

public class CustomerData : BaseEntity
{
    [Display(Name = "Customer Name")]
    [Required(ErrorMessage = "Customer name wajib diisi")]
    public string CustomerName { get; set; } = string.Empty;

    [Display(Name = "Customer Type")]
    [Required(ErrorMessage = "Customer type wajib diisi")]
    public string CustomerType { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
