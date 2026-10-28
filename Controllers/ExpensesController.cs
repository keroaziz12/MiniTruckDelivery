using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTruckDelivery.Date;
using MiniTruckDelivery.Models;

namespace MiniTruckDelivery.Controllers;

[Authorize(Roles = "Driver")]
public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExpensesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<Driver?> GetCurrentDriverAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _context.Drivers
            .SingleOrDefaultAsync(driver => driver.UserId == userId && driver.IsActive);
    }

    [HttpGet]
    public async Task<IActionResult> Add(int tripId)
    {
        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        var ownsTrip = await _context.Trips
            .AnyAsync(trip =>
                trip.Id == tripId && trip.DriverId == driver.Id && !trip.IsCancelled);

        if (!ownsTrip)
        {
            return NotFound();
        }

        return View(new TripExpense
        {
            TripId = tripId,
            ExpenseDate = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(TripExpense expense)
    {
        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        var ownsTrip = await _context.Trips
            .AnyAsync(trip =>
                trip.Id == expense.TripId && trip.DriverId == driver.Id);

        if (!ownsTrip)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(expense);
        }

        _context.TripExpenses.Add(expense);
        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Trips",
            new { id = expense.TripId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        var expense = await _context.TripExpenses
            .Include(item => item.Trip)
            .FirstOrDefaultAsync(item =>
                item.Id == id &&
                item.Trip.DriverId == driver.Id && !item.Trip.IsCancelled);

        if (expense == null)
        {
            return NotFound();
        }

        var tripId = expense.TripId;

        _context.TripExpenses.Remove(expense);
        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Trips",
            new { id = tripId });
    }
}
