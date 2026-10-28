using System.ComponentModel.DataAnnotations;

namespace MiniTruckDelivery.Models.ViewModels;

public sealed class ComplaintViewModel
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Subject { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string ReplyEmail { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}
