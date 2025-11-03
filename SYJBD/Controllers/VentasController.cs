using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using SVJBD.Models.Ventas;
using SYJBD.Models;
using SYJBD.Models.Ventas;
using SYJBD.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Globalization;


namespace SYJBD.Controllers
{
    [Authorize]
    [Route("Ventas")]
    public class VentasController : Controller
    {
        private readonly ICajaService _cajaService;
        private readonly IVentaService _ventaService;

        public VentasController(ICajaService cajaService, IVentaService ventaService)
        {
            _cajaService = cajaService;
            _ventaService = ventaService;
        }

        // ======================= LISTA ===========================
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index([FromQuery] VentaFiltroVM filtro)
        {
            ViewData["Title"] = "Lista de ventas";

            if ((filtro.Desde.HasValue && !filtro.Hasta.HasValue) ||
                (!filtro.Desde.HasValue && filtro.Hasta.HasValue))
            {
                ViewBag.WarnFechas = "Debes seleccionar ambas fechas (Desde y Hasta) para filtrar por rango.";
                filtro.Desde = null;
                filtro.Hasta = null;
            }

            var combos = await _ventaService.GetCombosAsync();
            var (paged, totalMonto) = await _ventaService.ListarAsync(filtro);

            var vm = new VentaListaVM
            {
                Filtro = filtro,
                Paged = paged,
                TotalMonto = totalMonto,
                Combos = combos
            };
            return View("Index", vm);
        }

        // =================== PUNTO DE VENTA (Cajas) =============
        [HttpGet("PuntoDeVenta")]
        public async Task<IActionResult> PuntoDeVenta(int page = 1, int pageSize = 10)
        {
            var model = await _cajaService.ListarAsync(page, pageSize);
            ViewData["Title"] = "Cajas de venta";
            return View(model);
        }

        [HttpGet("Ver/{id:int}")]
        public async Task<IActionResult> Ver(int id)
        {
            var rol = User.IsInRole("COMERCIAL") ? "COMERCIAL"
                     : User.IsInRole("ADMINISTRADOR") ? "ADMINISTRADOR"
                     : null;

            var vm = await _cajaService.GetReporteAsync(id, rol);
            return PartialView("~/Views/Ventas/Partials/_CajaReporte.cshtml", vm);
        }

        // GET /Ventas/VerVenta/63  -> abre modal con lectura de venta
        [HttpGet("VerVenta/{id:int}")]
        public async Task<IActionResult> VerVenta(int id)
        {
            var vm = await _ventaService.GetComprobanteAsync(id);
            if (vm == null) return NotFound();

            // OJO: el archivo se llama _VentaDetalle.cshtml
            return PartialView("~/Views/Ventas/Partials/_VentaDetalle.cshtml", vm);
        }




        // ====================== NUEVA VENTA ======================
        // GET: /Ventas/NuevaVenta?idCaja=7&idTienda=TD1&idUsuario=CMR001
        [HttpGet("NuevaVenta")]
        public async Task<IActionResult> NuevaVenta([FromQuery] int idCaja, [FromQuery] string idTienda, [FromQuery] string idUsuario)
        {
            var combos = await _ventaService.GetCombosNuevaVentaAsync(idTienda, idUsuario, idCaja);
            ViewBag.Combos = combos;
            ViewData["Title"] = "Nuevo comprobante";
            return View("~/Views/Ventas/NuevaVenta.cshtml");
        }

        // ====================== COMPROBANTES =====================
        [HttpGet("Ticket80/{id:int}")]
        public async Task<IActionResult> Ticket80Html(int id)
        {
            var comp = await _ventaService.GetComprobanteAsync(id);
            if (comp == null) return NotFound();
            return View("~/Views/Ventas/Ticket80.cshtml", comp);
        }

        [HttpGet("TicketA4/{id:int}")]
        public async Task<IActionResult> TicketA4Html(int id)
        {
            var comp = await _ventaService.GetComprobanteAsync(id);
            if (comp == null) return NotFound();
            return View("~/Views/Ventas/TicketA4.cshtml", comp);
        }

        [HttpGet("Ticket80Pdf/{id:int}")]
        public async Task<IActionResult> Ticket80Pdf(int id)
        {
            var comp = await _ventaService.GetComprobanteAsync(id);
            if (comp is null) return NotFound();
            comp.IsPdf = true;

            return new ViewAsPdf("~/Views/Ventas/Ticket80.cshtml", comp)
            {
                FileName = $"Ticket_{comp.Serie}-{comp.IdVenta}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline
            };
        }

        [HttpGet("TicketA4Pdf/{id:int}")]
        public async Task<IActionResult> TicketA4Pdf(int id)
        {
            var comp = await _ventaService.GetComprobanteAsync(id);
            if (comp is null) return NotFound();
            comp.IsPdf = true;

            return new ViewAsPdf("~/Views/Ventas/TicketA4.cshtml", comp)
            {
                FileName = $"Boleta_{comp.Serie}-{comp.IdVenta}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline
            };
        }

        [HttpGet("Imprimir/{id:int}")]
        public async Task<IActionResult> Imprimir(int id)
        {
            var comp = await _ventaService.GetComprobanteAsync(id);
            if (comp == null) return NotFound();
            return PartialView("~/Views/Ventas/Partials/_ElegirFormato.cshtml", comp);
        }


        // ==== APIs ligeras para la pantalla NuevaVenta ====

        // GET /Ventas/TomarCorrelativo?tipo=NV|B|F
        [HttpGet("TomarCorrelativo")]
        public async Task<IActionResult> TomarCorrelativo([FromQuery] string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return BadRequest("tipo requerido");
            var nro = await _ventaService.TomarCorrelativoAsync(tipo);
            return Ok(new { correlativo = nro });
        }

        // GET /api/ventas/buscar-clientes?q=texto
        [HttpGet("/api/ventas/buscar-clientes")]
        public async Task<IActionResult> ApiBuscarClientes([FromQuery] string? q)
        {
            var lista = await _ventaService.BuscarClientesAsync(q ?? "");
            return Ok(lista);
        }

        // GET /api/ventas/buscar-productos?q=texto&talla=M&und=UND
        [HttpGet("/api/ventas/buscar-productos")]
        public async Task<IActionResult> ApiBuscarProductos(
            [FromQuery] string? q,
            [FromQuery] string? talla,
            [FromQuery] string? und,
            CancellationToken ct)
        {
            // El servicio actual acepta (string query, CancellationToken)
            var lista = await _ventaService.BuscarProductosAsync(q ?? "", ct);

            // (Opcional) si quieres aplicar talla/und del lado del cliente/servidor más tarde,
            // aquí puedes filtrarlo en memoria cuando amplíes el DTO.
            // Por ahora lo devolvemos tal cual para que compile y funcione.
            return Ok(lista);
        }


        // ================ REGISTRAR VENTA =================
        // POST /Ventas/Crear  (JSON body = VentaCreateVM)
        [HttpPost("Crear")]
        public async Task<IActionResult> Crear([FromBody] VentaCreateVM dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var id = await _ventaService.RegistrarVentaAsync(dto);
            return Ok(new { idVenta = id });
        }

        [HttpGet("LectorCodeBar")]
        public IActionResult LectorCodeBar()
        {
            return PartialView("~/Views/Ventas/Partials/_LectorCodeBar.cshtml");
        }




        // ====================== MODALES (Partials) ======================
        // Cliente
        [HttpGet("Modal/ClienteSelector")]
        public IActionResult ModalClienteSelector()
        {
            // Si necesitas defaults, envíalos por ViewBag
            return PartialView("~/Views/Ventas/Partials/_ClienteSelector.cshtml");
        }

        // Productos
        [HttpGet("Modal/ProductoSelector")]
        public IActionResult ModalProductoSelector()
        {
            return PartialView("~/Views/Ventas/Partials/_ProductoSelector.cshtml");
        }

        // Pago Depósito (Yape/Plin/BCP/...)
        [HttpGet("Modal/PagoDeposito")]
        public IActionResult ModalPagoDeposito([FromQuery] decimal total = 0m)
        {
            ViewBag.Total = total;
            return PartialView("~/Views/Ventas/Partials/_PagoDepositoModal.cshtml");
        }

        // Pago Mixto (dos métodos)
        [HttpGet("Modal/PagoMixto")]
        public IActionResult ModalPagoMixto([FromQuery] decimal total = 0m)
        {
            ViewBag.Total = total;
            return PartialView("~/Views/Ventas/Partials/_PagoMixtoModal.cshtml");
        }

        // GET /Ventas/Anular/123  → devuelve el partial con el resumen
        [HttpGet("Anular/{id:int}")]
        public async Task<IActionResult> AnularPreview(int id)
        {
            var vm = await _ventaService.GetAnularPreviewAsync(id);
            if (vm is null) return NotFound();
            return PartialView("~/Views/Ventas/Partials/_AnularResumen.cshtml", vm);
        }

        // POST /Ventas/Anular/123  → ejecuta la transacción y devuelve JSON
        [HttpPost("Anular/{id:int}")]
        public async Task<IActionResult> AnularConfirm(int id)
        {
            var (ok, msg) = await _ventaService.AnularVentaAsync(id);
            if (!ok) return BadRequest(new { ok, msg });
            return Ok(new { ok, msg });
        }
        // GET /Ventas/CajaAbrir  (abre el modal)
        [HttpGet("/Ventas/CajaAbrir")]
        public async Task<IActionResult> CajaAbrir()
        {
            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var vm = new CajaAbrirVM
            {
                IdUsuario = idUsuario,
                NextIdCaja = await _ventaService.GetNextIdCajaAsync(),
                Tiendas = await _ventaService.ListarTiendasAsync()
            };
            return PartialView("~/Views/Ventas/Partials/_CajaAbrir.cshtml", vm);
        }

        // POST /Ventas/CajaAbrir  (registra y devuelve JSON para redirigir)
        [HttpPost("/Ventas/CajaAbrir")]
        public async Task<IActionResult> CajaAbrir([FromForm] CajaAbrirPost dto)
        {
            if (dto is null
                || string.IsNullOrWhiteSpace(dto.IdTienda)
                || string.IsNullOrWhiteSpace(dto.IdUsuario)
                || dto.MontoApertura < 0m)   // cero es válido; negativo no
            {
                return Ok(new { ok = false, msg = "Datos incompletos o inválidos." });
            }

            var (ok, msg, idCaja) = await _ventaService.AbrirCajaAsync(
                dto.IdTienda, dto.IdUsuario, dto.MontoApertura);

            if (!ok) return Ok(new { ok, msg });

            // URL para redirigir a NuevaVenta
            var url = Url.Action("NuevaVenta", "Ventas", new
            {
                idCaja = idCaja,
                idTienda = dto.IdTienda,
                idUsuario = dto.IdUsuario
            });

            return Ok(new
            {
                ok = true,
                msg = "Caja aperturada.",
                idCaja,
                idTienda = dto.IdTienda,
                idUsuario = dto.IdUsuario,
                url
            });
        }

        public class CajaAbrirPost
        {
            public string IdTienda { get; set; } = "";
            public string IdUsuario { get; set; } = "";
            public decimal MontoApertura { get; set; }
        }

        // GET /Ventas/CajaCerrar?idCaja=7   -> devuelve el parcial para el modal
        [HttpGet("Ventas/CajaCerrar")]
        public async Task<IActionResult> CajaCerrar([FromQuery] int idCaja, CancellationToken ct = default)
        {
            var vm = await _ventaService.GetDatosCierreAsync(idCaja, ct);
            vm.IdUsuarioCierre = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            return PartialView("~/Views/Ventas/Partials/_CajaCerrar.cshtml", vm);
        }

        // POST /Ventas/CajaCerrar
        [HttpPost("Ventas/CajaCerrar")]
        public async Task<IActionResult> CajaCerrar([FromForm] CajaCerrarVM vm, CancellationToken ct = default)
        {
            try
            {
                if (vm == null || vm.IdCaja <= 0)
                    return Ok(new { ok = false, msg = "Datos incompletos." });

                // normalizar efectivo con punto
                var raw = (Request.Form["EfectivoReal"].FirstOrDefault() ?? "").Replace(",", ".");
                if (!decimal.TryParse(raw, System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture, out var efectivo))
                    return Ok(new { ok = false, msg = "Efectivo contado inválido (usa punto como decimal)." });

                if (efectivo < 0)
                    return Ok(new { ok = false, msg = "El efectivo contado no puede ser negativo." });

                // *** CLAVE: id del usuario que cierra, en formato US0000xx ***
                var userIdCodigo = (vm.IdUsuarioCierre ??
                                   User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                   User.Identity?.Name ?? "").Trim();

                var (ok, msg) = await _ventaService.CerrarCajaAsync(
                    vm.IdCaja,
                    userIdCodigo,           // <-- aquí va el código de usuario
                    efectivo,
                    vm.Observacion ?? "",
                    ct
                );

                return Ok(new { ok, msg });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cerrar la caja: {ex.Message}");
            }
        }

    }

}