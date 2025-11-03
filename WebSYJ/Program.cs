using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;



var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1) EF Core + MySQL (Pomelo)
// ----------------------------------------------------
var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException(
             "Falta ConnectionStrings:Default en appsettings.json");


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


// APP (si la quieres con prefijo /app)
app.MapControllerRoute(
    name: "app",
    pattern: "app/{controller=Ventas}/{action=PuntoDeVenta}/{id?}");

// WEBSITE por defecto ("/")
app.MapControllerRoute(
    name: "website",
    pattern: "{controller=Website}/{action=Index}/{id?}");

app.Run();
