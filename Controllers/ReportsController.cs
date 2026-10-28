using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTruckDelivery.Date;
using MiniTruckDelivery.Models;

namespace MiniTruckDelivery.Controllers;

[Authorize(Roles = "Driver")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<Driver?> GetCurrentDriverAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _context.Drivers.SingleOrDefaultAsync(driver => driver.UserId == userId && driver.IsActive);
    }

    public async Task<IActionResult> DailySummary(DateTime? date)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return Forbid();
        }

        var selectedDate = (date ?? DateTime.Today).Date;
        var nextDate = selectedDate.AddDays(1);
        var trips = await _context.Trips
            .Where(trip => trip.DriverId == driver.Id && !trip.IsCancelled && trip.TripDate >= selectedDate && trip.TripDate < nextDate)
            .Include(trip => trip.Driver)
            .Include(trip => trip.Expenses)
            .OrderBy(trip => trip.TripDate)
            .ToListAsync();

        return View(new ReportViewModel
        {
            Date = selectedDate,
            PeriodStart = selectedDate,
            PeriodEnd = selectedDate,
            Trips = trips
        });
    }

    public async Task<IActionResult> WeeklySummary(DateTime? date)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return Forbid();
        }

        var selectedDate = (date ?? DateTime.Today).Date;
        var daysSinceMonday = ((int)selectedDate.DayOfWeek + 6) % 7;
        var weekStart = selectedDate.AddDays(-daysSinceMonday);
        var weekEndExclusive = weekStart.AddDays(7);

        var trips = await _context.Trips
            .Where(trip => trip.DriverId == driver.Id && !trip.IsCancelled && trip.TripDate >= weekStart && trip.TripDate < weekEndExclusive)
            .Include(trip => trip.Driver)
            .Include(trip => trip.Expenses)
            .OrderBy(trip => trip.TripDate)
            .ToListAsync();

        return View(new ReportViewModel
        {
            Date = selectedDate,
            PeriodStart = weekStart,
            PeriodEnd = weekEndExclusive.AddDays(-1),
            Trips = trips
        });
    }
}

