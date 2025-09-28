using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SYJBD.Data;
using SYJBD.Models;

namespace SYJBD.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ErpDbContext _db;
        public ProductosController(ErpDbContext db) => _db = db;

        // GET: /Productos?q=texto&page=1
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 15)
        {
            if (page < 1) page = 1;

            var queryable = _db.Productos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                queryable = queryable.Where(p => p.Nombre != null && p.Nombre.Contains(term));
            }

            var total = await queryable.CountAsync();

            var items = await queryable
                .OrderBy(p => p.IdProducto)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new PagedResult<Producto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Query = q
            };

            return View(vm);
        }
    }
}
