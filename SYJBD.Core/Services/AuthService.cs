using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SYJBD.Data;
using SYJBD.Models;

namespace SYJBD.Services
{
    public class AuthService
    {
        private readonly UsersRepository _repo;
        private readonly IHttpContextAccessor _http;

        public AuthService(UsersRepository repo, IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        public Task<User?> ValidateAsync(string userOrEmail, string pwd)
            => _repo.ValidateAsync(userOrEmail, pwd);

        public async Task SignInAsync(User u, bool remember = false)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, u.IdUsuario),
        new Claim(ClaimTypes.Name, $"{u.Nombre} {u.Apellido}".Trim()),
        new Claim(ClaimTypes.Role, u.Rol ?? "COMERCIAL") // <-- importantísimo
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await _http.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = remember });
        }


        public Task SignOutAsync() =>
            _http.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
