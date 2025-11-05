using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SYJBD.Data;
using SYJBD.Models;
using SYJBD.Services;

namespace SYJBD.Controllers
{
    [Authorize(Roles = "COMERCIAL,ADMINISTRADOR")]
    public class ProductosController : Controller
    {
        private readonly ErpDbContext _db;
        private readonly IProductosService _svc;
        private readonly IBarcodeLayoutService _barcodeLayouts;
        public ProductosController(ErpDbContext db, IProductosService svc, IBarcodeLayoutService barcodeLayouts)
        {
            _db = db;
            _svc = svc;
            _barcodeLayouts = barcodeLayouts;
        }

        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 5;
            if (pageSize > 100) pageSize = 100;

            var query = _db.Productos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();

                // Si es numérico, busca por ID que "empiece con"
                if (int.TryParse(term, out _))
                {
                    query = query.Where(p => p.IdProducto.ToString().StartsWith(term));
                }
                else
                {
                    // AND de tokens en cualquier orden (cada palabra debe aparecer)
                    var tokens = term
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .ToArray();

                    foreach (var t in tokens)
                    {
                        var tt = t; // evita el closure sobre la variable del foreach
                        query = query.Where(p =>
                            p.Nombre != null &&
                            EF.Functions.Like(p.Nombre, "%" + tt + "%"));
                    }
                }
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.IdProducto)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new SYJBD.Models.PagedResult<SYJBD.Models.Producto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Query = q
            };

            return View(vm);
        }


        // ======= MODAL: Precios por talla =======
        [HttpGet]
        public async Task<IActionResult> EditarPrecios(int id, CancellationToken ct = default)
        {
            var prod = await _db.Productos.AsNoTracking()
                .Where(p => p.IdProducto == id)
                .Select(p => new { p.IdProducto, p.Nombre })
                .FirstOrDefaultAsync(ct);

            if (prod == null)
                return PartialView("_MsgSimple", "Producto no encontrado.");

            ViewBag.IdProducto = prod.IdProducto;
            ViewBag.NombreProducto = prod.Nombre ?? "";

            // OJO: acá usa _svc (tu servicio inyectado), no _productosService
            var modelo = await _svc.GetTallasPreciosAsync(prod.IdProducto, ct);

            ViewBag.ImpresionFormatos = _barcodeLayouts.GetLayouts();
            ViewBag.ImpresionImpresoras = _barcodeLayouts.GetPrinters();
            ViewBag.ImpresionAbierta = string.Equals(
                Request.Query["view"],
                "print",
                StringComparison.OrdinalIgnoreCase);

            return PartialView("_PreciosPorTalla", modelo);
        }

        [HttpGet]
        public async Task<IActionResult> ImpresionEtiquetas(int idProducto, string idProductoTalla, string formatoId, int copias = 1, string? impresora = null, CancellationToken ct = default)
        {
            var vm = new BarcodePrintPageVM
            {
                Copias = Math.Max(1, Math.Min(500, copias)),
                ImpresoraId = impresora
            };

            var layout = _barcodeLayouts.FindLayout(formatoId);
            if (layout == null)
            {
                vm.ErrorMessage = "No se encontro el formato de etiqueta solicitado.";
                return View("ImpresionEtiquetas", vm);
            }

            vm.Layout = layout;

            var detalle = await _svc.GetProductoTallaAsync(idProducto, idProductoTalla, ct);
            if (detalle == null)
            {
                vm.ErrorMessage = "No se encontro la talla seleccionada para este producto.";
                return View("ImpresionEtiquetas", vm);
            }

            vm.Detalle = detalle;

            var printer = _barcodeLayouts.GetPrinters()
                .FirstOrDefault(p => string.Equals(p.Id, impresora, StringComparison.OrdinalIgnoreCase));

            vm.ImpresoraNombre = printer?.DisplayName ?? impresora ?? string.Empty;

            return View("ImpresionEtiquetas", vm);
        }

        // ======= POST: ACTUALIZAR PRECIO =======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPrecio([FromBody] PrecioUpdateVM dto, CancellationToken ct)
        {
            if (dto == null || dto.IdProducto <= 0 || string.IsNullOrWhiteSpace(dto.IdTalla))
                return Ok(new { ok = false, msg = "Datos incompletos." });

            var (ok, msg) = await _svc.ActualizarPrecioAsync(dto.IdProducto, dto.IdTalla.Trim(), dto.Precio, ct);
            return Ok(new { ok, msg });
        }

        // DTO para el POST AJAX
        public class PrecioUpdateVM
        {
            public int IdProducto { get; set; }
            public string IdTalla { get; set; } = "";
            public decimal Precio { get; set; }
        }
  }
    }
