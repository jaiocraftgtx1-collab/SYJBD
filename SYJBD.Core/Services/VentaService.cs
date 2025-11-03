using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using SVJBD.Models.Ventas;
using SYJBD.Data;
using SYJBD.Models;
using SYJBD.Models.Ventas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace SYJBD.Services
{
    public class VentaService : IVentaService
    {
        private readonly ErpDbContext _db;
        public VentaService(ErpDbContext db) => _db = db;

        // =========================================================
        // Helpers ADO
        // =========================================================
        private static async Task EnsureOpenAsync(MySqlConnection cn, CancellationToken ct = default)
        {
            if (cn.State != ConnectionState.Open)
                await cn.OpenAsync(ct);
        }

        private static void AddParam(MySqlCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static string? S(MySqlDataReader rd, int i) => rd.IsDBNull(i) ? null : rd.GetString(i);
        private static int I(MySqlDataReader rd, int i) => rd.IsDBNull(i) ? 0 : rd.GetInt32(i);
        private static decimal D(MySqlDataReader rd, int i) => rd.IsDBNull(i) ? 0m : rd.GetDecimal(i);
        private static DateTime T(MySqlDataReader rd, int i) => rd.IsDBNull(i) ? DateTime.MinValue : rd.GetDateTime(i);

        // =========================================================
        // COMBOS (LISTA)
        // =========================================================
        public async Task<VentaCombosVM> GetCombosAsync()
        {
            var tiendas = new List<string> { "TODOS" };
            var vendedores = new List<string> { "TODOS" };
            var clientes = new List<string> { "TODOS" };
            var tiposDoc = new List<string> { "TODOS", "BOLETA ELECTRÓNICA", "FACTURA ELECTRÓNICA", "NOTA DE VENTA", "TICKET DE VENTA" };
            var estados = new List<string> { "TODOS", "ATENDIDO", "ANULADO" };

            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn);

            using (var c1 = cn.CreateCommand())
            {
                c1.CommandText = "SELECT nombre FROM vta_tienda ORDER BY nombre;";
                using var rd = await c1.ExecuteReaderAsync();
                while (await rd.ReadAsync()) tiendas.Add(rd.GetString(0));
            }

            using (var c2 = cn.CreateCommand())
            {
                c2.CommandText = "SELECT nombre FROM Usuarios ORDER BY nombre;";
                using var rd = await c2.ExecuteReaderAsync();
                while (await rd.ReadAsync()) vendedores.Add(rd.GetString(0));
            }

            using (var c3 = cn.CreateCommand())
            {
                c3.CommandText = "SELECT razon_social FROM Clientes ORDER BY razon_social;";
                using var rd = await c3.ExecuteReaderAsync();
                while (await rd.ReadAsync()) clientes.Add(rd.GetString(0));
            }

            return new VentaCombosVM
            {
                Tiendas = tiendas,
                Vendedores = vendedores,
                Clientes = clientes,
                TiposDoc = tiposDoc,
                Estados = estados
            };
        }

        // =========================================================
        // LISTADO (ya lo tenías; sin cambios)
        // =========================================================
        public async Task<(PagedResult<VentaItemVM> Paged, decimal TotalMonto)> ListarAsync(VentaFiltroVM f)
        {
            var items = new List<VentaItemVM>();
            decimal total = 0m;

            int page = f.Page <= 0 ? 1 : f.Page;
            int pageSize = f.PageSize <= 0 ? 10 : f.PageSize;
            int offset = (page - 1) * pageSize;

            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn);

            var where = " WHERE 1=1 ";
            var pars = new List<(string, object?)>();

            if (f.Desde.HasValue && f.Hasta.HasValue)
            {
                where += " AND v.fecha_venta BETWEEN @d1 AND @d2 ";
                pars.Add(("@d1", f.Desde.Value.Date));
                pars.Add(("@d2", f.Hasta.Value.Date.AddDays(1).AddTicks(-1)));
            }
            if (!string.IsNullOrWhiteSpace(f.Tienda) && f.Tienda != "TODOS") { where += " AND t.nombre=@tienda "; pars.Add(("@tienda", f.Tienda)); }
            if (!string.IsNullOrWhiteSpace(f.Vendedor) && f.Vendedor != "TODOS") { where += " AND u.nombre=@vend "; pars.Add(("@vend", f.Vendedor)); }
            if (!string.IsNullOrWhiteSpace(f.Cliente) && f.Cliente != "TODOS") { where += " AND c.razon_social=@cli "; pars.Add(("@cli", f.Cliente)); }
            if (!string.IsNullOrWhiteSpace(f.TipoDoc) && f.TipoDoc != "TODOS") { where += " AND v.tipo_documento=@td "; pars.Add(("@td", f.TipoDoc)); }
            if (!string.IsNullOrWhiteSpace(f.Estado) && f.Estado != "TODOS") { where += " AND v.estado_venta=@est "; pars.Add(("@est", f.Estado)); }

            int totalItems = 0;
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = $@"
SELECT COUNT(1), COALESCE(SUM(v.total),0)
FROM vta_venta v
JOIN vta_tienda t ON t.id_tienda=v.id_tienda
JOIN Clientes c ON c.id_cliente=v.id_cliente
JOIN Usuarios u ON u.id_usuario=v.id_usuario
{where};";
                foreach (var p in pars) AddParam(cmd, p.Item1, p.Item2);

                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync()) { totalItems = rd.GetInt32(0); total = rd.GetDecimal(1); }
            }

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = $@"
SELECT v.fecha_venta, v.serie, v.id_venta, v.total,
       u.nombre AS vendedor, c.razon_social AS cliente,
       v.id_caja, t.nombre AS tienda, v.estado_venta,
       v.numero_doc              -- NUEVO (posición 9)
FROM vta_venta v
JOIN vta_tienda t ON t.id_tienda = v.id_tienda
JOIN Clientes   c ON c.id_cliente = v.id_cliente
JOIN Usuarios   u ON u.id_usuario = v.id_usuario
{where}
ORDER BY v.fecha_venta DESC, v.id_venta DESC
LIMIT @lim OFFSET @off;
";
                foreach (var p in pars) AddParam(cmd, p.Item1, p.Item2);
                AddParam(cmd, "@lim", pageSize);
                AddParam(cmd, "@off", offset);

                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    items.Add(new VentaItemVM
                    {
                        // LO QUE YA TENÍAS (mismos índices)
                        FechaVenta = rd.GetDateTime(0).ToString("dd/MM/yyyy HH:mm"),
                        Serie = S(rd, 1) ?? "",
                        Nro = rd.GetInt32(2),
                        Monto = D(rd, 3),
                        Vendedor = S(rd, 4) ?? "",
                        Cliente = S(rd, 5) ?? "",
                        Caja = I(rd, 6),
                        Tienda = S(rd, 7) ?? "",
                        Estado = S(rd, 8) ?? "",

                        // NUEVO sin romper nada
                        IdVenta = rd.GetInt32(2),        // reutiliza el mismo índice
                        NumeroDoc = S(rd, 9) ?? ""
                    });

                }
            }

            var paged = new PagedResult<VentaItemVM>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
            return (paged, total);
        }

        // =========================================================
        // COMPROBANTE (ya lo tenías)
        // =========================================================
        public async Task<VentaComprobanteVM?> GetComprobanteAsync(int idVenta)
        {
            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn);

            VentaComprobanteVM? vm = null;

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT 
    v.id_venta, v.tipo_documento, v.serie, v.correlativo, v.fecha_venta,
    v.estado_venta, v.estado, v.id_caja, v.id_tienda,
    v.id_cliente, v.id_usuario,
    c.razon_social, c.ruc,
    t.nombre, u.nombre,
    v.mp1, v.monto_mp1, v.mp2, v.monto_mp2, v.total
FROM vta_venta v
JOIN vta_tienda t ON t.id_tienda=v.id_tienda
JOIN Clientes   c ON c.id_cliente=v.id_cliente
JOIN Usuarios   u ON u.id_usuario=v.id_usuario
WHERE v.id_venta=@id;";
                AddParam(cmd, "@id", idVenta);

                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    vm = new VentaComprobanteVM
                    {
                        IdVenta = I(rd, 0),
                        TipoDocumento = S(rd, 1) ?? "",
                        Serie = S(rd, 2) ?? "",
                        Correlativo = I(rd, 3),
                        FechaVenta = T(rd, 4),
                        EstadoVenta = S(rd, 5) ?? "",
                        Estado = S(rd, 6) ?? "",
                        IdCaja = I(rd, 7),
                        IdTienda = S(rd, 8) ?? "",
                        IdCliente = S(rd, 9) ?? "",
                        IdUsuario = S(rd, 10) ?? "",
                        ClienteNombre = S(rd, 11) ?? "",
                        ClienteRuc = S(rd, 12) ?? "",
                        TiendaNombre = S(rd, 13) ?? "",
                        VendedorNombre = S(rd, 14) ?? "",
                        MP1 = S(rd, 15),
                        MontoMP1 = D(rd, 16),
                        MP2 = S(rd, 17),
                        MontoMP2 = D(rd, 18),
                        Total = D(rd, 19),
                        Items = new List<VentaItemDetalleVM>()
                    };
                }
            }
            if (vm == null) return null;

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT k.id_producto, p.nombre, k.id_talla, k.id_unidad,
       k.cantidad_movida, k.costo_unitario, k.costo_total
FROM vta_kardex k
JOIN vta_producto p ON p.id_producto=k.id_producto
WHERE k.id_origen=@id AND k.tipo_movimiento='SV'
ORDER BY k.id_kardex;";
                AddParam(cmd, "@id", vm.IdVenta);

                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    vm.Items.Add(new VentaItemDetalleVM
                    {
                        IdProducto = I(rd, 0),
                        Nombre = S(rd, 1) ?? "",
                        IdTalla = S(rd, 2) ?? "",
                        IdUnidad = S(rd, 3) ?? "",
                        Cantidad = D(rd, 4),
                        PrecioUnitario = D(rd, 5),
                        Total = D(rd, 6)
                    });
                }
            }

            vm.IgvTasa = 0.18m;
            vm.Igv = Math.Round(vm.Total * vm.IgvTasa, 2);
            vm.SubTotal = Math.Round(vm.Total - vm.Igv, 2);
            return vm;
        }

        // =========================================================
        // ======= NUEVA VENTA =====================================
        // =========================================================
        public async Task<VentaCombosVM> GetCombosNuevaVentaAsync(string idTienda, string idUsuario, int idCaja, CancellationToken ct = default)
        {
            var combos = new VentaCombosVM
            {
                IdTienda = idTienda,
                IdCaja = idCaja,
                IdUsuario = idUsuario,
                SerieNTV = "NV",
                SerieBLT = "B001",
                SerieFCT = "F001",
                IdClienteGenerico = "SYJ0007",
                RucGenerico = "77777777",
                NombreGenerico = "CLIENTE SYJ STYLE"
            };

            return await Task.FromResult(combos);
        }

        // Services/VentaService.cs
        public async Task<IEnumerable<dynamic>> BuscarClientesAsync(string query, CancellationToken ct = default)
        {
            var list = new List<dynamic>();
            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
        SELECT id_cliente, ruc, razon_social
        FROM Clientes
        WHERE tipo_cliente = 'TIENDA'
          AND estado = 'ACTIVO'
          AND (ruc LIKE CONCAT('%', @q, '%') OR razon_social LIKE CONCAT('%', @q, '%'))
        ORDER BY razon_social
        LIMIT 50;";
            AddParam(cmd, "@q", query ?? "");

            using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                // ¡OJO!: aquí forzamos las CLAVES de salida => id, nombre, ruc
                dynamic o = new ExpandoObject();
                o.id = rd["id_cliente"]?.ToString() ?? "";
                o.nombre = rd["razon_social"]?.ToString() ?? "";
                o.ruc = rd["ruc"]?.ToString() ?? "";
                list.Add(o);
            }

            return list;
        }


        public async Task<IEnumerable<dynamic>> BuscarProductosAsync(string query, CancellationToken ct = default)
        {
            var list = new List<dynamic>();
            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            // mismo orden que GetTallasPreciosAsync
            const string ORDER_TALLAS =
                "'T-0','T-1','T-2','T-4','T-6','T-8','T-10','T-12','T-14','T-16'," +
                "'T-S','T-M','T-L','T-XL','T-XXL','T-GRD','T-MED','T-PEQ','T-STD'";

            using var cmd = cn.CreateCommand();
            cmd.CommandText = $@"
SELECT p.id_producto, p.nombre, pt.id_talla,
       p.id_unidad, COALESCE(pt.precio,0) AS precio
FROM vta_producto p
JOIN vta_producto_talla pt ON pt.id_producto = p.id_producto
WHERE UPPER(TRIM(p.estado))='ACTIVO'
/*##WHERE_Q##*/
ORDER BY p.id_producto DESC, FIELD(pt.id_talla, {ORDER_TALLAS})
LIMIT 80;";

            // --- FILTRO por q: tokeniza y exige TODAS las palabras ---
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Palabras “limpias”
                var toks = query.Trim()
                                .ToUpperInvariant()
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Texto “combinado” a buscar
                var combo = "CONCAT(UPPER(p.nombre),' ',CAST(p.id_producto AS CHAR),' ',UPPER(pt.id_talla),' ',UPPER(p.id_unidad))";

                // AND ( combo LIKE '%w0%' AND combo LIKE '%w1%' AND ... )
                var ands = new List<string>();
                for (int i = 0; i < toks.Length; i++)
                {
                    ands.Add($"{combo} LIKE CONCAT('%', @w{i}, '%')");
                    cmd.Parameters.AddWithValue($"@w{i}", toks[i]);
                }

                cmd.CommandText = cmd.CommandText.Replace("/*##WHERE_Q##*/", "AND (" + string.Join(" AND ", ands) + ")");
            }
            else
            {
                cmd.CommandText = cmd.CommandText.Replace("/*##WHERE_Q##*/", "");
            }

            using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                dynamic o = new ExpandoObject();
                o.idProducto = rd.GetInt32(0);
                o.nombre = rd.GetString(1);
                o.idTalla = rd.GetString(2);
                o.idUnidad = rd.GetString(3);
                o.precioUnit = rd.GetDecimal(4);
                list.Add(o);
            }
            return list;
        }





        // using System.Data;   // asegúrate de tenerlo arriba
        // using MySqlConnector;

        private static async Task<int> TomarCorrelativoCoreAsync(
            MySqlConnection cn,
            MySqlTransaction? tx,
            string tipoDocumento,
            CancellationToken ct = default)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "sp_tomar_correlativo";
            cmd.CommandType = CommandType.StoredProcedure;

            var pTipo = cmd.Parameters.Add("@p_tipo_documento", MySqlDbType.VarChar, 5);
            pTipo.Direction = ParameterDirection.Input;
            pTipo.Value = tipoDocumento;

            var pOut = cmd.Parameters.Add("@p_correlativo", MySqlDbType.Int32);
            pOut.Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync(ct);
            return (pOut.Value == null || pOut.Value == DBNull.Value) ? 0 : Convert.ToInt32(pOut.Value);
        }

        // Sobrecarga pública (para el endpoint GET /Ventas/TomarCorrelativo)
        public async Task<int> TomarCorrelativoAsync(string tipoDocumento, CancellationToken ct = default)
        {
            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);
            // sin transacción (tx = null) porque solo queremos consultar
            return await TomarCorrelativoCoreAsync(cn, null, tipoDocumento, ct);
        }



        public async Task<int> RegistrarVentaAsync(VentaCreateVM vm, CancellationToken ct = default)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (vm.Items == null || vm.Items.Count == 0) throw new InvalidOperationException("La venta no tiene items.");

            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            using var tx = await cn.BeginTransactionAsync(ct);
            try
            {
                // 1) Correlativo dentro de la misma conexión/transacción
                int corr = await TomarCorrelativoCoreAsync(cn, (MySqlTransaction)tx, vm.TipoDocumento, ct);
                if (corr <= 0) throw new InvalidOperationException("No se pudo obtener correlativo.");

                // 2) INSERT vta_venta
                int idVenta;
                using (var cmd = cn.CreateCommand())
                {
                    cmd.Transaction = (MySqlTransaction)tx;
                    cmd.CommandText = @"
INSERT INTO vta_venta
(tipo_documento, serie, correlativo, fecha_venta,
 id_cliente, id_usuario, id_tienda, id_caja,
 mp1, monto_mp1, mp2, monto_mp2, total, estado_venta, estado)
VALUES
(@td, @serie, @cor, @fv,
 @cli, @usr, @tie, @caj,
 @mp1, @m1, @mp2, @m2, @tot, 'ATENDIDO', 'ACTIVO');";
                    AddParam(cmd, "@td", vm.TipoDocumento);
                    AddParam(cmd, "@serie", vm.Serie);
                    AddParam(cmd, "@cor", corr);
                    AddParam(cmd, "@fv", vm.FechaVenta);
                    AddParam(cmd, "@cli", vm.IdCliente);
                    AddParam(cmd, "@usr", vm.IdUsuario);
                    AddParam(cmd, "@tie", vm.IdTienda);
                    AddParam(cmd, "@caj", vm.IdCaja);
                    AddParam(cmd, "@mp1", vm.MP1 ?? "-");
                    AddParam(cmd, "@m1", vm.MontoMP1);
                    AddParam(cmd, "@mp2", vm.MP2 ?? "-");
                    AddParam(cmd, "@m2", vm.MontoMP2);
                    AddParam(cmd, "@tot", vm.Total);
                    await cmd.ExecuteNonQueryAsync(ct);
                }

                using (var cmd = cn.CreateCommand())
                {
                    cmd.Transaction = (MySqlTransaction)tx;
                    cmd.CommandText = "SELECT LAST_INSERT_ID();";
                    var o = await cmd.ExecuteScalarAsync(ct);
                    idVenta = Convert.ToInt32(o);
                }

                // 3) INSERT KÁRDEX (salida por venta)
                string nroDoc = $"{vm.Serie}-{corr}";
                foreach (var it in vm.Items)
                {
                    using var kd = cn.CreateCommand();
                    kd.Transaction = (MySqlTransaction)tx;
                    kd.CommandText = @"
INSERT INTO vta_kardex
(tipo_movimiento, tipo_ref, doc_ref, nro_doc, id_origen, fecha_movimiento,
 id_producto, id_talla, id_unidad, cantidad_movida, costo_unitario, costo_total,
 moneda, tipo_cambio, id_usuario, estado)
VALUES
('SV', @td, @dr, @nro, @idv, @fmov,
 @prod, @talla, @und, @cant, @precio, @total,
 'SOLES', 1.0000, @usr, 'ACTIVO');";
                    AddParam(kd, "@td", vm.TipoDocumento);
                    AddParam(kd, "@dr", vm.TipoDocumento switch
                    {
                        "BLT" => "BOLETA ELECTRÓNICA",
                        "FCT" => "FACTURA ELECTRÓNICA",
                        "NTV" => "NOTA DE VENTA",
                        "TKT" => "TICKET DE VENTA",
                        _ => vm.TipoDocumento
                    });
                    AddParam(kd, "@nro", nroDoc);
                    AddParam(kd, "@idv", idVenta);
                    AddParam(kd, "@fmov", vm.FechaVenta);
                    AddParam(kd, "@prod", it.IdProducto);
                    AddParam(kd, "@talla", it.IdTalla);
                    AddParam(kd, "@und", it.IdUnidad);
                    AddParam(kd, "@cant", it.Cantidad);
                    AddParam(kd, "@precio", it.PrecioUnit);
                    AddParam(kd, "@total", Math.Round(it.Cantidad * it.PrecioUnit, 2));
                    AddParam(kd, "@usr", vm.IdUsuario);
                    await kd.ExecuteNonQueryAsync(ct);
                }

                await tx.CommitAsync(ct);
                return idVenta;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<AnularVentaPreviewVM?> GetAnularPreviewAsync(int idVenta, CancellationToken ct = default)
        {
            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            // 1) Cabecera
            string? serie = null;
            int corr = 0;
            DateTime fecha = DateTime.MinValue;
            decimal total = 0m;
            string ev = "", es = "";

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT serie, correlativo, fecha_venta, COALESCE(total,0), 
       UPPER(COALESCE(estado_venta,'')), UPPER(COALESCE(estado,''))
FROM vta_venta
WHERE id_venta=@id;";
                AddParam(cmd, "@id", idVenta);
                using var rd = await cmd.ExecuteReaderAsync(ct);
                if (!await rd.ReadAsync(ct))
                    return new AnularVentaPreviewVM { NotFound = true };

                serie = S(rd, 0) ?? "";
                corr = I(rd, 1);
                fecha = T(rd, 2);
                total = D(rd, 3);
                ev = S(rd, 4) ?? "";
                es = S(rd, 5) ?? "";
            }

            // 2) Conteo de kardex SV activos
            int nActivos = 0;
            using (var cmd2 = cn.CreateCommand())
            {
                cmd2.CommandText = @"
SELECT COUNT(*) 
FROM vta_kardex
WHERE id_origen=@id AND tipo_movimiento='SV' AND estado<>'INACTIVO';";
                AddParam(cmd2, "@id", idVenta);
                var o = await cmd2.ExecuteScalarAsync(ct);
                nActivos = Convert.ToInt32(o);
            }

            return new AnularVentaPreviewVM
            {
                IdVenta = idVenta,
                NumeroDoc = string.IsNullOrWhiteSpace(serie) ? corr.ToString() : $"{serie}-{corr}",
                Fecha = fecha,
                Total = total,
                EstadoVenta = ev,
                Estado = es,
                KardexSvActivos = nActivos
            };
        }

        public async Task<(bool Ok, string Msg)> AnularVentaAsync(int idVenta, CancellationToken ct = default)
        {
            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            using var tx = await cn.BeginTransactionAsync(ct);
            try
            {
                // 0) Revalidar que exista y no esté anulada
                string ev = "", es = "";
                using (var c0 = cn.CreateCommand())
                {
                    c0.Transaction = (MySqlTransaction)tx;
                    c0.CommandText = "SELECT UPPER(COALESCE(estado_venta,'')), UPPER(COALESCE(estado,'')) FROM vta_venta WHERE id_venta=@id;";
                    AddParam(c0, "@id", idVenta);
                    using var rd = await c0.ExecuteReaderAsync(ct);
                    if (!await rd.ReadAsync(ct)) return (false, "La venta no existe.");
                    ev = S(rd, 0) ?? "";
                    es = S(rd, 1) ?? "";
                }
                if (ev == "ANULADO" || es == "INACTIVO") return (false, "La venta ya está anulada.");

                // 1) Inactivar KÁRDEX SV
                using (var c1 = cn.CreateCommand())
                {
                    c1.Transaction = (MySqlTransaction)tx;
                    c1.CommandText = @"
UPDATE vta_kardex 
   SET estado='INACTIVO'
 WHERE id_origen=@id AND tipo_movimiento='SV' AND estado<>'INACTIVO';";
                    AddParam(c1, "@id", idVenta);
                    await c1.ExecuteNonQueryAsync(ct);
                }

                // Verificación: ya no deben quedar SV activos
                int quedan = 0;
                using (var c1v = cn.CreateCommand())
                {
                    c1v.Transaction = (MySqlTransaction)tx;
                    c1v.CommandText = @"
SELECT COUNT(*) 
FROM vta_kardex 
WHERE id_origen=@id AND tipo_movimiento='SV' AND estado<>'INACTIVO';";
                    AddParam(c1v, "@id", idVenta);
                    var o = await c1v.ExecuteScalarAsync(ct);
                    quedan = Convert.ToInt32(o);
                }
                if (quedan != 0)
                {
                    await tx.RollbackAsync(ct);
                    return (false, "No se pudieron inactivar todos los movimientos de kárdex (SV).");
                }

                // 2) Marcar venta ANULADO/INACTIVO
                using (var c2 = cn.CreateCommand())
                {
                    c2.Transaction = (MySqlTransaction)tx;
                    c2.CommandText = "UPDATE vta_venta SET estado_venta='ANULADO', estado='INACTIVO' WHERE id_venta=@id;";
                    AddParam(c2, "@id", idVenta);
                    await c2.ExecuteNonQueryAsync(ct);
                }

                // Verificación
                using (var c2v = cn.CreateCommand())
                {
                    c2v.Transaction = (MySqlTransaction)tx;
                    c2v.CommandText = "SELECT UPPER(estado_venta), UPPER(estado) FROM vta_venta WHERE id_venta=@id;";
                    AddParam(c2v, "@id", idVenta);
                    using var rd = await c2v.ExecuteReaderAsync(ct);
                    if (!await rd.ReadAsync(ct))
                    {
                        await tx.RollbackAsync(ct);
                        return (false, "No se encontró la venta tras actualizar.");
                    }
                    var ev2 = S(rd, 0) ?? "";
                    var es2 = S(rd, 1) ?? "";
                    if (ev2 != "ANULADO" || es2 != "INACTIVO")
                    {
                        await tx.RollbackAsync(ct);
                        return (false, "La venta no quedó ANULADA/INACTIVA. Revise triggers o permisos.");
                    }
                }

                await tx.CommitAsync(ct);
                return (true, "Venta anulada correctamente.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return (false, "Fallo la anulación: " + ex.Message);
            }
        }

        // ====================== ID siguiente de caja ======================
        public async Task<int> GetNextIdCajaAsync(CancellationToken ct = default)
        {
            var cs = _db.Database.GetConnectionString();
            await using var cn = new MySqlConnection(cs);
            await cn.OpenAsync(ct);

            await using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(MAX(id_caja),0)+1 FROM vta_caja;";
            var v = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(v);
        }

        // ====================== Listado de tiendas ========================
        public async Task<IEnumerable<Tienda>> ListarTiendasAsync(CancellationToken ct = default)
        {
            var cs = _db.Database.GetConnectionString();
            await using var cn = new MySqlConnection(cs);
            await cn.OpenAsync(ct);

            var list = new List<Tienda>();
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT id_tienda, nombre, ubicacion FROM vta_tienda ORDER BY id_tienda;";

            await using var rd = await cmd.ExecuteReaderAsync(ct);
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

        // Abrir caja: una CAJA ABIERTA por tienda a la vez, usando hora de Lima (America/Lima)
        public async Task<(bool Ok, string Msg, int IdCaja)> AbrirCajaAsync(
            string idTienda, string idUsuarioApertura, decimal montoApertura, CancellationToken ct = default)
        {
            await _db.Database.OpenConnectionAsync(ct);
            using var cn = _db.Database.GetDbConnection();
            using var tx = await cn.BeginTransactionAsync(ct);

            try
            {
                // 1) Validar: ¿ya hay una caja ABIERTA para esta tienda?
                using (var c0 = cn.CreateCommand())
                {
                    c0.Transaction = tx;
                    c0.CommandText = @"
                SELECT 1
                FROM vta_caja
                WHERE id_tienda = @tid
                  AND fecha_cierre IS NULL
                LIMIT 1;";
                    var pTid = c0.CreateParameter(); pTid.ParameterName = "@tid"; pTid.Value = idTienda;
                    c0.Parameters.Add(pTid);

                    var exists = await c0.ExecuteScalarAsync(ct);
                    if (exists != null)
                    {
                        await tx.RollbackAsync(ct);
                        return (false, "Ya existe una caja abierta para esta tienda.", 0);
                    }
                }

                // 2) Insertar (fecha/hora de Lima)
                using (var c1 = cn.CreateCommand())
                {
                    c1.Transaction = tx;
                    c1.CommandText = @"
INSERT INTO vta_caja
  (id_tienda, id_usuario_apertura, fecha_apertura, monto_apertura, observacion)
VALUES
  (@tid, @usr, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 HOUR), @monto, 'CAJA ABIERTA');";


                    var pTid = c1.CreateParameter(); pTid.ParameterName = "@tid"; pTid.Value = idTienda;
                    var pUsr = c1.CreateParameter(); pUsr.ParameterName = "@usr"; pUsr.Value = idUsuarioApertura;
                    var pMonto = c1.CreateParameter(); pMonto.ParameterName = "@monto"; pMonto.Value = montoApertura;

                    c1.Parameters.Add(pTid);
                    c1.Parameters.Add(pUsr);
                    c1.Parameters.Add(pMonto);

                    await c1.ExecuteNonQueryAsync(ct);
                }

                // 3) Obtener id_caja generado (por tienda)
                int idCaja = 0;
                using (var c2 = cn.CreateCommand())
                {
                    c2.Transaction = tx;
                    c2.CommandText = @"
                SELECT MAX(id_caja) FROM vta_caja WHERE id_tienda=@t;";
                    var pt = c2.CreateParameter(); pt.ParameterName = "@t"; pt.Value = idTienda;
                    c2.Parameters.Add(pt);

                    idCaja = Convert.ToInt32(await c2.ExecuteScalarAsync(ct));
                }

                await tx.CommitAsync(ct);
                return (true, "OK", idCaja);
            }
            catch (Exception ex)
            {
                try { await tx.RollbackAsync(ct); } catch { /* ignore */ }
                return (false, "Error al abrir la caja: " + ex.Message, 0);
            }
        }

        public async Task<CajaCerrarVM> GetDatosCierreAsync(int idCaja, CancellationToken ct = default)
        {
            var vm = new CajaCerrarVM { IdCaja = idCaja };

            await _db.Database.OpenConnectionAsync(ct);
            using var cn = _db.Database.GetDbConnection();

            // 1) Datos base de la caja
            using (var c = cn.CreateCommand())
            {
                c.CommandText = @"
                    SELECT id_tienda, id_usuario_apertura, fecha_apertura, monto_apertura
                    FROM vta_caja
                    WHERE id_caja = @id;";
                var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);

                using var rd = await c.ExecuteReaderAsync(ct);
                if (await rd.ReadAsync(ct))
                {
                    vm.IdTienda = rd.IsDBNull(0) ? "" : rd.GetString(0);
                    vm.IdUsuarioApertura = rd.IsDBNull(1) ? "" : rd.GetString(1);
                    vm.FechaApertura = rd.IsDBNull(2) ? DateTime.Now : rd.GetDateTime(2);
                    vm.MontoApertura = rd.IsDBNull(3) ? 0 : rd.GetDecimal(3);
                }
                rd.Close();
            }

            // 2) Ingresos en EFECTIVO (por MP1/MP2)
            using (var c = cn.CreateCommand())
            {
                c.CommandText = @"
                    SELECT IFNULL(SUM(
                        CASE WHEN UPPER(mp1)='EFECTIVO' THEN monto_mp1 ELSE 0 END
                      + CASE WHEN UPPER(mp2)='EFECTIVO' THEN monto_mp2 ELSE 0 END
                    ),0)
                    FROM vta_venta
                    WHERE id_caja = @id
                      AND estado='ACTIVO' AND estado_venta='ATENDIDO';";
                var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);

                vm.TotalIngresosEfectivo = Convert.ToDecimal(await c.ExecuteScalarAsync(ct) ?? 0m);
            }

            // 3) Gastos EFECTIVO autorizados
            using (var c = cn.CreateCommand())
            {
                c.CommandText = @"
                    SELECT IFNULL(SUM(monto),0)
                    FROM T_Egresos
                    WHERE id_caja = @id
                      AND UPPER(metodo_pago)='EFECTIVO'
                      AND UPPER(estado)='AUTORIZADO';";
                var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);

                vm.TotalGastosEfectivoAut = Convert.ToDecimal(await c.ExecuteScalarAsync(ct) ?? 0m);
            }

            // 3b) Conteo pendientes
            using (var c = cn.CreateCommand())
            {
                c.CommandText = @"
                    SELECT COUNT(*)
                    FROM T_Egresos
                    WHERE id_caja = @id
                      AND UPPER(metodo_pago)='EFECTIVO'
                      AND UPPER(estado)<>'AUTORIZADO';";
                var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);

                vm.EgresosPendientes = Convert.ToInt32(await c.ExecuteScalarAsync(ct) ?? 0);
            }

            // 4) Efectivo esperado
            vm.EfectivoEsperado = vm.MontoApertura + vm.TotalIngresosEfectivo - vm.TotalGastosEfectivoAut;

            return vm;
        }

        public async Task<(bool Ok, string Msg)> CerrarCajaAsync(
             int idCaja, string idUsuarioCierre, decimal efectivoReal, string observacion, CancellationToken ct = default)
        {
            await _db.Database.OpenConnectionAsync(ct);
            using var cn = _db.Database.GetDbConnection();
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                // 0) Validar que NO existan egresos EFECTIVO pendientes
                int pendientes = 0;
                using (var c = cn.CreateCommand())
                {
                    c.Transaction = tx.GetDbTransaction();
                    c.CommandText = @"
                        SELECT COUNT(*) FROM T_Egresos
                        WHERE id_caja=@id
                          AND UPPER(metodo_pago)='EFECTIVO'
                          AND UPPER(estado)<>'AUTORIZADO';";
                    var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);
                    pendientes = Convert.ToInt32(await c.ExecuteScalarAsync(ct) ?? 0);
                }
                if (pendientes > 0)
                {
                    await tx.RollbackAsync(ct);
                    return (false, "No puedes cerrar la caja: hay egresos pendientes de autorización.");
                }

                // 1) Recalcular totales confiables
                decimal totalVentas = 0m, totalGastos = 0m, apertura = 0m;

                using (var c = cn.CreateCommand())
                {
                    c.Transaction = tx.GetDbTransaction();
                    c.CommandText = @"
                        SELECT IFNULL(SUM(total),0)
                        FROM vta_venta
                        WHERE id_caja=@id
                          AND estado='ACTIVO' AND estado_venta='ATENDIDO';";
                    var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);
                    totalVentas = Convert.ToDecimal(await c.ExecuteScalarAsync(ct) ?? 0m);
                }

                using (var c = cn.CreateCommand())
                {
                    c.Transaction = tx.GetDbTransaction();
                    c.CommandText = @"
                        SELECT IFNULL(SUM(monto),0)
                        FROM T_Egresos
                        WHERE id_caja=@id
                          AND UPPER(metodo_pago)='EFECTIVO'
                          AND UPPER(estado)='AUTORIZADO';";
                    var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);
                    totalGastos = Convert.ToDecimal(await c.ExecuteScalarAsync(ct) ?? 0m);
                }

                using (var c = cn.CreateCommand())
                {
                    c.Transaction = tx.GetDbTransaction();
                    c.CommandText = @"SELECT IFNULL(monto_apertura,0) FROM vta_caja WHERE id_caja=@id;";
                    var p = c.CreateParameter(); p.ParameterName = "@id"; p.Value = idCaja; c.Parameters.Add(p);
                    apertura = Convert.ToDecimal(await c.ExecuteScalarAsync(ct) ?? 0m);
                }

                var esperado = vmRound(apertura) + vmRound(totalVentas) - vmRound(totalGastos);
                var diferencia = esperado - vmRound(efectivoReal);

                // 1b) Componer observación final (prefijo + nota opcional)
                string prefijo = diferencia == 0m
                    ? "CIERRE CORRECTO"
                    : (diferencia > 0m
                        ? $"OBSERVADO (FALTA {Math.Abs(diferencia):0.00})"
                        : $"OBSERVADO (EXCEDE {Math.Abs(diferencia):0.00})");
                string obsFinal = string.IsNullOrWhiteSpace(observacion)
                    ? prefijo
                    : $"{prefijo} — {observacion.Trim()}";

                // 2) UPDATE vta_caja
                using (var c = cn.CreateCommand())
                {
                    c.Transaction = tx.GetDbTransaction();
                    c.CommandText = @"
                        UPDATE vta_caja SET
                            id_usuario_cierre = @usr,
                            fecha_cierre      = DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 HOUR),
                            monto_cierre      = @real,
                            total_ingresos    = @totV,
                            total_gastos      = @totG,
                            efectivo_esperado = @esp,
                            diferencia_caja   = @dif,
                            observacion       = @obs
                        WHERE id_caja = @id;";
                    var p1 = c.CreateParameter(); p1.ParameterName = "@usr"; p1.Value = idUsuarioCierre; c.Parameters.Add(p1);
                    var p2 = c.CreateParameter(); p2.ParameterName = "@real"; p2.Value = vmRound(efectivoReal); c.Parameters.Add(p2);
                    var p3 = c.CreateParameter(); p3.ParameterName = "@totV"; p3.Value = vmRound(totalVentas); c.Parameters.Add(p3);
                    var p4 = c.CreateParameter(); p4.ParameterName = "@totG"; p4.Value = vmRound(totalGastos); c.Parameters.Add(p4);
                    var p5 = c.CreateParameter(); p5.ParameterName = "@esp"; p5.Value = vmRound(esperado); c.Parameters.Add(p5);
                    var p6 = c.CreateParameter(); p6.ParameterName = "@dif"; p6.Value = vmRound(diferencia); c.Parameters.Add(p6);
                    var p7 = c.CreateParameter(); p7.ParameterName = "@obs"; p7.Value = obsFinal; c.Parameters.Add(p7);
                    var p8 = c.CreateParameter(); p8.ParameterName = "@id"; p8.Value = idCaja; c.Parameters.Add(p8);

                    await c.ExecuteNonQueryAsync(ct);
                }

                await tx.CommitAsync(ct);
                return (true, "Cierre de caja exitoso.");
            }
            catch (Exception ex)
            {
                try { await tx.RollbackAsync(ct); } catch { }
                return (false, "Error al cerrar la caja: " + ex.Message);
            }

            static decimal vmRound(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
        }
    }
}
