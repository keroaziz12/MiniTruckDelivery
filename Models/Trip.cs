using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;


namespace MiniTruckDelivery.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string EndLocation { get; set; } = string.Empty;

        [Range(0, 1_000_000)]
        public decimal AgreedAmount { get; set; }

        public DateTime TripDate { get; set; } = DateTime.Today;

        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }

        public int DriverId { get; set; }
        [ValidateNever]
        public Driver Driver { get; set; } = null!;
        public ICollection<TripExpense> Expenses { get; set; } = new List<TripExpense>();
    }
}


