using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniTruckDelivery.Models.ViewModels;

namespace MiniTruckDelivery.Controllers;

[Authorize(Roles = "Driver")]
public sealed class SupportController : Controller
{
    [HttpGet]
    public IActionResult Contact() => View(new ComplaintViewModel());
}
