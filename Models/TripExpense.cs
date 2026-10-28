using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MiniTruckDelivery.Models
{
    public class TripExpense
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ExpenseType { get; set; } = string.Empty;

        [Range(0.01, 1_000_000)]
        public decimal Amount { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        public int TripId { get; set; }
        [ValidateNever]

        public Trip Trip { get; set; } = null!;
    }
}
