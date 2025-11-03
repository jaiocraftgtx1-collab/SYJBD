using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SYJBD.Data;
using SYJBD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SYJBD.Models.CajaReporteVM;

namespace SYJBD.Services
{
    public class CajaService : ICajaService
    {
        private readonly ErpDbContext _db;

        public CajaService(ErpDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<CajaListadoVM>> ListarAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var baseQ = _db.Cajas
                .AsNoTracking()
                .OrderByDescending(c => c.FechaApertura);

            var total = await baseQ.CountAsync();

            var items = await baseQ
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CajaListadoVM
                {
                    IdCaja = c.IdCaja,
                    IdTienda = c.IdTienda,
                    IdUsuarioApertura = c.IdUsuarioApertura,
                    FechaApertura = c.FechaApertura,
                    IdUsuarioCierre = c.IdUsuarioCierre,
                    FechaCierre = c.FechaCierre,
                    Observacion = c.Observacion ?? ""
                })
                .ToListAsync();

            return new PagedResult<CajaListadoVM>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = items
            };
        }

        public async Task<CajaReporteVM> GetReporteAsync(int idCaja, string? rolUsuario)
        {
            // 1) Cabecera
            var cab = await (from c in _db.Cajas.AsNoTracking()
                             where c.IdCaja == idCaja
                             join t in _db.Tiendas.AsNoTracking() on c.IdTienda equals t.IdTienda into tj
                             from t in tj.DefaultIfEmpty()
                             select new CajaReporteVM.CabeceraVM
                             {
                                 IdCaja = c.IdCaja,
                                 IdTienda = c.IdTienda,
                                 TiendaNombre = t != null ? t.Nombre : "",
                                 IdUsuarioApertura = c.IdUsuarioApertura,
                                 FechaApertura = c.FechaApertura,
                                 IdUsuarioCierre = c.IdUsuarioCierre,
                                 FechaCierre = c.FechaCierre,
                                 Observacion = c.Observacion ?? "",
                                 MontoApertura = c.MontoApertura,
                                 MontoCierre = c.MontoCierre
                             }).FirstOrDefaultAsync();

            if (cab == null)
                throw new InvalidOperationException($"No existe la caja {idCaja}.");

            // 2) Métodos de Pago (ATENDIDO + ACTIVO)
            var qMp1 = _db.Ventas.AsNoTracking()
                .Where(v => v.IdCaja == idCaja && v.Estado == "ACTIVO" && v.EstadoVenta == "ATENDIDO")
                .Select(v => new { Metodo = v.MP1, Monto = v.MontoMP1 ?? 0m });

            var qMp2 = _db.Ventas.AsNoTracking()
                .Where(v => v.IdCaja == idCaja && v.Estado == "ACTIVO" && v.EstadoVenta == "ATENDIDO")
                .Select(v => new { Metodo = v.MP2, Monto = v.MontoMP2 ?? 0m });

            var metodos = await qMp1.Concat(qMp2)
                .Where(x => x.Metodo != null && x.Metodo != "-")
                .GroupBy(x => x.Metodo!)
                .Select(g => new MetodoPagoVM
                {
                    Metodo = g.Key,
                    Total = g.Sum(x => x.Monto)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // 3) Egresos por caja
            var egresosQ =
                from e in _db.Egresos.AsNoTracking()
                where e.IdCaja == idCaja &&
                      (e.Estado == "PENDIENTE" || e.Estado == "AUTORIZADO")
                select new { e, t = (TipoEgreso?)null };

            if (rolUsuario?.ToUpperInvariant() == "COMERCIAL")
            {
                egresosQ =
                    from e in _db.Egresos.AsNoTracking()
                    join t in _db.TipoEgresos.AsNoTracking()
                        on e.IdTipoEgreso equals t.IdTipoEgreso into tj
                    from t in tj.DefaultIfEmpty()
                    where e.IdCaja == idCaja &&
                          (e.Estado == "PENDIENTE" || e.Estado == "AUTORIZADO") &&
                          (t != null && t.Tipo == "OPERATIVO")
                    select new { e, t };
            }

            var egresos = await egresosQ
                .OrderBy(x => x.e.FechaRegistro)
                .Select(x => new EgresoRowVM
                {
                    Codigo = x.e.IdEgreso,
                    Detalle = x.e.Detalle ?? "",
                    Monto = x.e.Monto ?? 0m,
                    MetodoPago = x.e.MetodoPago ?? "",
                    Fecha = x.e.FechaRegistro,
                    Estado = x.e.Estado ?? ""
                })
                .ToListAsync();

            // 4) TOP productos (por monto) — consulta 100% traducible por EF
            var topProductosQuery =
                from k in _db.Kardex
                join v in _db.Ventas on k.IdOrigen equals v.IdVenta
                where k.TipoMovimiento == "SV"
                   && k.Estado == "ACTIVO"
                   && v.Estado == "ACTIVO"
                   && v.EstadoVenta == "ATENDIDO"
                   && v.IdCaja == idCaja
                group k by new { k.IdProducto, k.IdTalla } into g
                select new
                {
                    g.Key.IdProducto,
                    g.Key.IdTalla,

                    // Cantidad: sumamos como decimal? y casteamos a int al final
                    Cantidad = (int)((g.Sum(x => (decimal?)x.CantidadMovida)) ?? 0m),

                    // Monto: sumamos como decimal?
                    Monto = (g.Sum(x => (decimal?)x.CostoTotal)) ?? 0m
                };

            var topProductos = await
                (from t in topProductosQuery
                 join p in _db.Productos on t.IdProducto equals p.IdProducto into gj
                 from p in gj.DefaultIfEmpty() // LEFT JOIN
                 orderby t.Monto descending
                 select new TopProductoVM
                 {
                     IdProducto = t.IdProducto,
                     Nombre = p != null ? p.Nombre : "",
                     IdTalla = t.IdTalla,
                     Cantidad = t.Cantidad,
                     Monto = t.Monto
                 })
                .Take(50)
                .ToListAsync();


            // 5) Contadores
            var cont = new ContadoresVM
            {
                VentasAtendidas = await _db.Ventas.AsNoTracking()
                    .Where(v => v.IdCaja == idCaja && v.Estado == "ACTIVO" && v.EstadoVenta == "ATENDIDO")
                    .CountAsync(),

                VentasAnuladas = await _db.Ventas.AsNoTracking()
                    .Where(v => v.IdCaja == idCaja && v.EstadoVenta == "ANULADO")
                    .CountAsync(),

                EgresosAut = await _db.Egresos.AsNoTracking()
                    .Where(e => e.IdCaja == idCaja && e.Estado == "AUTORIZADO")
                    .CountAsync(),

                EgresosPen = await _db.Egresos.AsNoTracking()
                    .Where(e => e.IdCaja == idCaja && e.Estado == "PENDIENTE")
                    .CountAsync(),

                EgresosAnu = await _db.Egresos.AsNoTracking()
                    .Where(e => e.IdCaja == idCaja && e.Estado == "ANULADO")
                    .CountAsync()
            };

            // 6) Ensamble del ViewModel
            return new CajaReporteVM
            {
                Cabecera = cab,
                MetodosPago = metodos,
                Egresos = egresos,
                TopProductos = topProductos,
                Contadores = cont
            };
        }

        public async Task<int> GetNextIdCajaAsync(CancellationToken ct = default)
        {
            await _db.EnsureOpenAsync(ct);
            var cn = _db.Database.GetDbConnection();   // ✅ sin using
            using var c = cn.CreateCommand();
            c.CommandText = "SELECT COALESCE(MAX(id_caja),0)+1 FROM vta_caja;";
            var v = await c.ExecuteScalarAsync(ct);
            return Convert.ToInt32(v);
        }


        public async Task<IEnumerable<Tienda>> ListarTiendasAsync(CancellationToken ct = default)
        {
            await _db.EnsureOpenAsync(ct);
            var list = new List<Tienda>();
            using var cn = _db.Database.GetDbConnection();
            using var c = cn.CreateCommand();
            c.CommandText = "SELECT id_tienda, nombre, ubicacion FROM vta_tienda ORDER BY id_tienda;";
            using var rd = await c.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                list.Add(new Tienda
                {
                    IdTienda = rd.GetString(0),
                    Nombre = rd.IsDBNull(1) ? "" : rd.GetString(1),
                    Ubicacion = rd.IsDBNull(2) ? "" : rd.GetString(2)
                });
            }
            return list;
        }

        public async Task<(bool Ok, string Msg, int IdCaja)> AbrirCajaAsync(
            string idTienda, string idUsuarioApertura, decimal montoApertura, CancellationToken ct = default)
        {
            await _db.EnsureOpenAsync(ct);

            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var dbTx = tx.GetDbTransaction(); // DbTransaction subyacente

            try
            {
                // 1) Validar si ya hay caja abierta hoy en la tienda
                using (var c1 = _db.Database.GetDbConnection().CreateCommand())
                {
                    c1.Transaction = dbTx;
                    c1.CommandText = @"
SELECT id_caja
FROM vta_caja
WHERE id_tienda=@t
  AND DATE(fecha_apertura)=CURDATE()
  AND fecha_cierre IS NULL
LIMIT 1;";
                    var p = c1.CreateParameter(); p.ParameterName = "@t"; p.Value = idTienda; c1.Parameters.Add(p);

                    var ex = await c1.ExecuteScalarAsync(ct);
                    if (ex != null)
                        return (false, "Ya existe una caja abierta para esta tienda hoy.", 0);
                }

                // 2) Insertar apertura
                using (var c2 = _db.Database.GetDbConnection().CreateCommand())
                {
                    c2.Transaction = dbTx;
                    c2.CommandText = @"
INSERT INTO vta_caja (id_tienda, id_usuario_apertura, fecha_apertura, monto_apertura, observacion)
VALUES (@t, @u, NOW(), @m, 'CAJA ABIERTA');";

                    var p1 = c2.CreateParameter(); p1.ParameterName = "@t"; p1.Value = idTienda; c2.Parameters.Add(p1);
                    var p2 = c2.CreateParameter(); p2.ParameterName = "@u"; p2.Value = idUsuarioApertura; c2.Parameters.Add(p2);
                    var p3 = c2.CreateParameter(); p3.ParameterName = "@m"; p3.Value = montoApertura; c2.Parameters.Add(p3);

                    await c2.ExecuteNonQueryAsync(ct);
                }

                // 3) Obtener id_caja generado
                int idCaja;
                using (var c3 = _db.Database.GetDbConnection().CreateCommand())
                {
                    c3.Transaction = dbTx;
                    c3.CommandText = "SELECT MAX(id_caja) FROM vta_caja WHERE id_tienda=@t;";
                    var p = c3.CreateParameter(); p.ParameterName = "@t"; p.Value = idTienda; c3.Parameters.Add(p);

                    idCaja = System.Convert.ToInt32(await c3.ExecuteScalarAsync(ct));
                }

                await tx.CommitAsync(ct);
                return (true, "OK", idCaja);
            }
            catch (System.Exception ex)
            {
                await tx.RollbackAsync(ct);
                return (false, "Error al abrir la caja: " + ex.Message, 0);
            }
        }
    }
}
