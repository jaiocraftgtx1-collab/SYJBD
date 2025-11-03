using Microsoft.EntityFrameworkCore;
using SYJBD.Data;
using SYJBD.Services;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException(
             "Falta ConnectionStrings:Default en appsettings.json");

builder.Services.AddDbContext<ErpDbContext>(opt =>
{
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IProductosService, ProductosService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Website/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Website}/{action=Index}/{id?}");

app.Run();
