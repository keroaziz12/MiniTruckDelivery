using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTruckDelivery.Date;
using MiniTruckDelivery.Models;

namespace MiniTruckDelivery.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var adminIds = (await _userManager.GetUsersInRoleAsync("Admin")).Select(user => user.Id).ToHashSet();
        var driverAccounts = await _userManager.GetUsersInRoleAsync("Driver");
        var model = new AdminDashboardViewModel
        {
            TotalUsers = (await _context.Users.CountAsync()) - adminIds.Count,
            DriverAccounts = driverAccounts.Count(user => !adminIds.Contains(user.Id)),
            DriverProfiles = await _context.Drivers.CountAsync(),
            UnlinkedDriverProfiles = await _context.Drivers.CountAsync(driver => driver.UserId == null)
        };
        return View(model);
    }
    public async Task<IActionResult> Drivers()
    {
        var adminIds = (await _userManager.GetUsersInRoleAsync("Admin")).Select(user => user.Id).ToHashSet();
        var users = await _context.Users.OrderBy(user => user.UserName).ToListAsync();
        var drivers = await _context.Drivers
            .Include(driver => driver.Trips)
            .ThenInclude(trip => trip.Expenses)
            .ToListAsync();
        var driversByUser = drivers
            .Where(driver => !string.IsNullOrWhiteSpace(driver.UserId))
            .ToDictionary(driver => driver.UserId!);
        var now = DateTimeOffset.UtcNow;

        var rows = users
            .Where(user => !adminIds.Contains(user.Id))
            .Select(user =>
            {
                driversByUser.TryGetValue(user.Id, out var driver);
                var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > now;
                return new AdminDriverRowViewModel
                {
                    Id = driver?.Id ?? 0,
                    DriverId = driver?.Id,
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = driver?.FullName ?? user.FullName,
                    PhoneNumber = driver?.PhoneNumber ?? user.PhoneNumber ?? string.Empty,
                    VehicleType = driver?.VehicleType ?? "Not provided",
                    HasAccount = true,
                    HasProfile = driver != null,
                    HasAccess = !isLocked && (driver?.IsActive ?? true),
                    TripsCount = driver?.Trips.Count ?? 0,
                    TotalExpenses = driver?.Trips.SelectMany(trip => trip.Expenses).Sum(expense => expense.Amount) ?? 0m,
                    IsActive = driver?.IsActive ?? true
                };
            })
            .ToList();

        return View(rows);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetUserAccess(string userId, bool allowAccess)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (await _userManager.IsInRoleAsync(user, "Admin")) return Forbid();

        await _userManager.SetLockoutEnabledAsync(user, true);
        var lockoutResult = await _userManager.SetLockoutEndDateAsync(
            user,
            allowAccess ? null : DateTimeOffset.MaxValue);
        if (!lockoutResult.Succeeded)
        {
            TempData["AdminMessage"] = string.Join("; ", lockoutResult.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Drivers));
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        var driver = await _context.Drivers.SingleOrDefaultAsync(item => item.UserId == user.Id);
        if (driver != null) driver.IsActive = allowAccess;
        await _context.SaveChangesAsync();

        TempData["AdminMessage"] = allowAccess
            ? "Account access enabled."
            : "Account access disabled. The user will not be able to sign in, and existing trip records were retained.";
        return RedirectToAction(nameof(Drivers));
    }
    public async Task<IActionResult> DriverDetails(int id)
    {
        var driver = await _context.Drivers
            .Where(item => item.Id == id)
            .Select(item => new AdminDriverDetailsViewModel
            {
                Id = item.Id,
                FullName = item.FullName,
                PhoneNumber = item.PhoneNumber,
                VehicleType = item.VehicleType,
                HasAccount = item.UserId != null,
                IsActive = item.IsActive,
                TripsCount = item.Trips.Count,
                TotalExpenses = item.Trips.SelectMany(trip => trip.Expenses).Sum(expense => (decimal?)expense.Amount) ?? 0m,
                Trips = item.Trips.OrderByDescending(trip => trip.TripDate).Select(trip => new AdminTripRowViewModel
                {
                    Id = trip.Id,
                    CustomerName = trip.CustomerName,
                    DriverName = item.FullName,
                    StartLocation = trip.StartLocation,
                    EndLocation = trip.EndLocation,
                    TripDate = trip.TripDate,
                    AgreedAmount = trip.AgreedAmount,
                    TotalExpenses = trip.Expenses.Sum(expense => (decimal?)expense.Amount) ?? 0m,
                    IsCancelled = trip.IsCancelled
                }).ToList()
            })
            .SingleOrDefaultAsync();

        return driver == null ? NotFound() : View(driver);
    }

    public async Task<IActionResult> Trips()
    {
        var trips = await _context.Trips
            .OrderByDescending(trip => trip.TripDate)
            .Select(trip => new AdminTripRowViewModel
            {
                Id = trip.Id,
                CustomerName = trip.CustomerName,
                DriverName = trip.Driver.FullName,
                StartLocation = trip.StartLocation,
                EndLocation = trip.EndLocation,
                TripDate = trip.TripDate,
                AgreedAmount = trip.AgreedAmount,
                TotalExpenses = trip.Expenses.Sum(expense => (decimal?)expense.Amount) ?? 0m,
                IsCancelled = trip.IsCancelled
            })
            .ToListAsync();
        return View(trips);
    }

    public async Task<IActionResult> TripDetails(int id)
    {
        var trip = await _context.Trips
            .Where(item => item.Id == id)
            .Select(item => new AdminTripDetailsViewModel
            {
                Id = item.Id,
                CustomerName = item.CustomerName,
                DriverName = item.Driver.FullName,
                StartLocation = item.StartLocation,
                EndLocation = item.EndLocation,
                TripDate = item.TripDate,
                AgreedAmount = item.AgreedAmount,
                IsCancelled = item.IsCancelled,
                CancelledAt = item.CancelledAt,
                Expenses = item.Expenses.OrderBy(expense => expense.ExpenseDate).ToList()
            })
            .SingleOrDefaultAsync();

        return trip == null ? NotFound() : View(trip);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelTrip(int id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null) return NotFound();

        if (!trip.IsCancelled)
        {
            trip.IsCancelled = true;
            trip.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        TempData["AdminMessage"] = "Trip cancelled. Its history and expenses were retained.";
        return RedirectToAction(nameof(TripDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreTrip(int id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null) return NotFound();

        trip.IsCancelled = false;
        trip.CancelledAt = null;
        await _context.SaveChangesAsync();
        TempData["AdminMessage"] = "Trip restored.";
        return RedirectToAction(nameof(TripDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        var trip = await _context.Trips.Include(item => item.Expenses).SingleOrDefaultAsync(item => item.Id == id);
        if (trip == null) return NotFound();

        _context.TripExpenses.RemoveRange(trip.Expenses);
        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();
        TempData["AdminMessage"] = "Trip and its expenses were permanently deleted.";
        return RedirectToAction(nameof(Trips));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDriverActive(int id, bool isActive)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null) return NotFound();

        driver.IsActive = isActive;
        await _context.SaveChangesAsync();
        TempData["AdminMessage"] = isActive ? "Driver account reactivated." : "Driver account deactivated. Existing records were retained.";
        return RedirectToAction(nameof(DriverDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (await _userManager.IsInRoleAsync(user, "Admin")) return Forbid();

        var driver = await _context.Drivers
            .Include(item => item.Trips)
            .SingleOrDefaultAsync(item => item.UserId == user.Id);

        if (driver?.Trips.Count > 0)
        {
            TempData["AdminMessage"] = "This account has trip history and cannot be deleted. Disable its access to preserve the trips and expenses.";
            return RedirectToAction(nameof(Drivers));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        if (driver != null)
        {
            driver.UserId = null;
            await _context.SaveChangesAsync();
            _context.Drivers.Remove(driver);
            await _context.SaveChangesAsync();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            TempData["AdminMessage"] = "The account could not be deleted: " + string.Join("; ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Drivers));
        }

        await transaction.CommitAsync();
        TempData["AdminMessage"] = "Account deleted permanently.";
        return RedirectToAction(nameof(Drivers));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDriver(int id)
    {
        var driver = await _context.Drivers.Include(item => item.Trips).SingleOrDefaultAsync(item => item.Id == id);
        if (driver == null) return NotFound();

        if (driver.Trips.Count > 0)
        {
            TempData["AdminMessage"] = "This driver has trip history. Deactivate the account to preserve financial records.";
            return RedirectToAction(nameof(DriverDetails), new { id });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        ApplicationUser? user = null;
        if (!string.IsNullOrWhiteSpace(driver.UserId))
        {
            user = await _userManager.FindByIdAsync(driver.UserId);
            driver.UserId = null;
            await _context.SaveChangesAsync();
        }

        _context.Drivers.Remove(driver);
        await _context.SaveChangesAsync();

        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                TempData["AdminMessage"] = "The driver profile could not be deleted: " + string.Join("; ", result.Errors.Select(error => error.Description));
                return RedirectToAction(nameof(Drivers));
            }
        }

        await transaction.CommitAsync();
        TempData["AdminMessage"] = "Driver profile deleted.";
        return RedirectToAction(nameof(Drivers));
    }
}



