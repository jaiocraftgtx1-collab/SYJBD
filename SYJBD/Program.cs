using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using SYJBD.Data;
using SYJBD.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1) EF Core + MySQL (Pomelo)
// ----------------------------------------------------
var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException(
             "Falta ConnectionStrings:Default en appsettings.json");

builder.Services.AddDbContext<ErpDbContext>(opt =>
{
    // AutoDetect obtiene la versión de MySQL/MariaDB para configurar el provider
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

// ----------------------------------------------------
// 2) MVC con política global de autorización
//    (todo requiere login, excepto [AllowAnonymous])
// ----------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// ----------------------------------------------------
// 3) Autenticación por Cookies
// ----------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Auth/Login";     // pantalla de login
        opt.AccessDeniedPath = "/Auth/Denied";
        opt.SlidingExpiration = true;
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// ----------------------------------------------------
// 4) Inyección de dependencias (solo lo que usas hoy)
//    *No* registramos servicios de Cajas/POS.
// ----------------------------------------------------
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// ----------------------------------------------------
// 5) Middleware pipeline
// ----------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------
// 6) Rutas
//    - Ruta por defecto: muestra el Login.
//    - Sin alias /Ventas/PuntoDeVenta (lo eliminamos).
// ----------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
