using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using SYJBD.Data;
using SYJBD.Services;
using Rotativa.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1) EF Core + MySQL (Pomelo)
// ----------------------------------------------------
var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException(
             "Falta ConnectionStrings:Default en appsettings.json");

builder.Services.AddDbContext<ErpDbContext>(opt =>
{
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

// ----------------------------------------------------
// 2) MVC con política global de autorización
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
        opt.LoginPath = "/Auth/Login";
        opt.AccessDeniedPath = "/Auth/Denied";
        opt.SlidingExpiration = true;
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();


// ----------------------------------------------------
// 4) Inyección de dependencias
// ----------------------------------------------------
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<AuthService>();

// Necesario para /Ventas/PuntoDeVenta
builder.Services.AddScoped<ICajaService, CajaService>();

builder.Services.AddScoped<IVentaService, VentaService>(); // ? NUEVO

builder.Services.AddScoped<IProductosService, ProductosService>();

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

Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath);

// APP (si la quieres con prefijo /app)
app.MapControllerRoute(
    name: "app",
    pattern: "app/{controller=Ventas}/{action=PuntoDeVenta}/{id?}");

// Ruta por defecto hacia la pantalla de inicio de sesión
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
