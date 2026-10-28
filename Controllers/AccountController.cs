using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniTruckDelivery.Date;
using MiniTruckDelivery.Models;
using MiniTruckDelivery.Models.ViewModels;

namespace MiniTruckDelivery.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        DriverRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber
        };

        var createResult =
            await _userManager.CreateAsync(user, model.Password);

        if (!createResult.Succeeded)
        {
            AddErrors(createResult);
            return View(model);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, "Driver");

        if (!roleResult.Succeeded)
        {
            AddErrors(roleResult);
            return View(model);
        }

        var driver = new Driver
        {
            UserId = user.Id,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            VehicleType = model.VehicleType,
            RegistrationDate = DateTime.Today
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Trips");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.UserName,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (user != null && await _userManager.IsInRoleAsync(user, "Driver"))
            {
                var isActiveDriver = await _context.Drivers.AnyAsync(driver => driver.UserId == user.Id && driver.IsActive);
                if (isActiveDriver)
                {
                    return RedirectToAction("Index", "Trips");
                }

                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "This driver account is inactive. Contact the administrator.");
                return View(model);
            }

            await _signInManager.SignOutAsync();
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "This account is temporarily locked. Try again later.");
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid username or password.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}




