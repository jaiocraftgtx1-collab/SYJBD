using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SYJBD.Models;
using SYJBD.Services;

namespace SYJBD.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth) => _auth = auth;

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Error = "Completa usuario y contraseña.";
                return View(vm);
            }

            var u = await _auth.ValidateAsync(vm.Usuario, vm.Contrasena);
            if (u is null)
            {
                vm.Error = "Usuario o contraseña inválidos.";
                return View(vm);
            }

            await _auth.SignInAsync(u);

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            // redirección por rol
            return u.Rol?.ToUpperInvariant() == "ADMINISTRADOR"
                ? RedirectToAction("Index", "Productos")
                : RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Logout   (para el <a> del sidebar, evita el 404)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Si tu AuthService implementa SignOutAsync, úsalo; si no, usa cookies directas:
            try
            {
                await _auth.SignOutAsync();
            }
            catch
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Login", "Auth");
        }

        // POST: /Auth/LogoutPost  (alternativa segura con AntiForgery)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            try
            {
                await _auth.SignOutAsync();
            }
            catch
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Login", "Auth");
        }

        // (Opcional) Vista de acceso denegado
        [HttpGet]
        public IActionResult Denied() => View();
    }
}
