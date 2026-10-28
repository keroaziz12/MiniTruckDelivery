using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTruckDelivery.Date;
using MiniTruckDelivery.Models;

namespace MiniTruckDelivery.Controllers;

[Authorize(Roles = "Driver")]
public class TripsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TripsController(ApplicationDbContext context)
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

    public async Task<IActionResult> Index()
    {
        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        var trips = await _context.Trips
            .Where(trip => trip.DriverId == driver.Id)
            .Include(trip => trip.Driver)
            .OrderByDescending(trip => trip.TripDate)
            .ToListAsync();

        return View(trips);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (await GetCurrentDriverAsync() == null) return Forbid();
        return View(new Trip { TripDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Trip trip)
    {
        if (!ModelState.IsValid)
        {
            return View(trip);
        }

        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        // نحدد السائق من الحساب المسجل، ولا نعتمد على قيمة يرسلها النموذج.
        trip.DriverId = driver.Id;

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        var trip = await _context.Trips
            .Include(item => item.Driver)
            .Include(item => item.Expenses)
            .FirstOrDefaultAsync(item =>
                item.Id == id && item.DriverId == driver.Id);

        if (trip == null)
        {
            return NotFound();
        }

        return View(trip);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        var trip = await _context.Trips
            .FirstOrDefaultAsync(item =>
                item.Id == id && item.DriverId == driver.Id && !item.IsCancelled);

        if (trip == null)
        {
            return NotFound();
        }

        return View(trip);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Trip trip)
    {
        if (id != trip.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(trip);
        }

        var driver = await GetCurrentDriverAsync();

        if (driver == null)
        {
            return Forbid();
        }

        var existingTrip = await _context.Trips
            .FirstOrDefaultAsync(item =>
                item.Id == id && item.DriverId == driver.Id && !item.IsCancelled);

        if (existingTrip == null)
        {
            return NotFound();
        }

        existingTrip.CustomerName = trip.CustomerName;
        existingTrip.StartLocation = trip.StartLocation;
        existingTrip.EndLocation = trip.EndLocation;
        existingTrip.AgreedAmount = trip.AgreedAmount;
        existingTrip.TripDate = trip.TripDate;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
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

        var trip = await _context.Trips
            .Include(item => item.Expenses)
            .FirstOrDefaultAsync(item =>
                item.Id == id && item.DriverId == driver.Id && !item.IsCancelled);

        if (trip == null)
        {
            return NotFound();
        }

        _context.TripExpenses.RemoveRange(trip.Expenses);
        _context.Trips.Remove(trip);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}


