using System.ComponentModel.DataAnnotations;

namespace MiniTruckDelivery.Models.ViewModels;

public class DriverRegisterViewModel
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(11)]
    [RegularExpression("^[0-9]{1,11}$")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string VehicleType { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(10)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}