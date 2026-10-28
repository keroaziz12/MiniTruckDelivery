using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace MiniTruckDelivery.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]

        [StringLength(11)]
        [RegularExpression(@"^[0-9]{1,11}$",
    ErrorMessage = "Phone number must contain up to 11 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty;
        public string? UserId { get; set; }

        [ValidateNever]
        public ApplicationUser? User { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}

