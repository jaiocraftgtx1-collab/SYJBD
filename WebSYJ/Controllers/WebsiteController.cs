using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebSYJ.Controllers;

[AllowAnonymous]
public class WebsiteController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Inicio";
        return View();
    }
}
