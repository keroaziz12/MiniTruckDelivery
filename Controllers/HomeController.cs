using Microsoft.AspNetCore.Mvc;

namespace MiniTruckDelivery.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (User.IsInRole("Driver"))
        {
            return RedirectToAction("Index", "Trips");
        }

        return View();
    }
}


