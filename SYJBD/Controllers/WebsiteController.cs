using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]                       // <- imprescindible
public class WebsiteController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Inicio";
        return View();
    }
}
