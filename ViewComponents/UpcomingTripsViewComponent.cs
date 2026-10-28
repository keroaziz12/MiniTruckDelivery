using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTruckDelivery.Date;
using MiniTruckDelivery.Models;

namespace MiniTruckDelivery.ViewComponents;

public sealed class UpcomingTripsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public UpcomingTripsViewComponent(ApplicationDbContext context) => _context = context;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var today = DateTime.Today;
        var through = today.AddDays(8);
        var trips = new List<Trip>();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var driverId = await _context.Drivers
                .Where(driver => driver.UserId == userId && driver.IsActive)
                .Select(driver => (int?)driver.Id)
                .SingleOrDefaultAsync();

            if (driverId.HasValue)
            {
                trips = await _context.Trips
                    .Where(trip => trip.DriverId == driverId.Value && !trip.IsCancelled && trip.TripDate >= today && trip.TripDate < through)
                    .OrderBy(trip => trip.TripDate)
                    .Take(6)
                    .ToListAsync();
            }
        }

        return View(trips);
    }
}



