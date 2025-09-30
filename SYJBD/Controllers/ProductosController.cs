using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SYJBD.Data;
using SYJBD.Models;

namespace SYJBD.Controllers
{
    [Authorize(Roles = "COMERCIAL,ADMINISTRADOR")]
    public class ProductosController : Controller
    {
        private readonly ErpDbContext _db;
        public ProductosController(ErpDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 5;
            if (pageSize > 100) pageSize = 100;

            var query = _db.Productos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                if (int.TryParse(term, out _))
                    query = query.Where(p => p.IdProducto.ToString().StartsWith(term));
                else
                    query = query.Where(p => p.Nombre != null && EF.Functions.Like(p.Nombre, $"%{term}%"));
            }

            var total = await query.CountAsync();

            var items = await query
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
