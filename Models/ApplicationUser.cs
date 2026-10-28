using Microsoft.AspNetCore.Identity;

namespace MiniTruckDelivery.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public Driver? DriverProfile { get; set; }
}