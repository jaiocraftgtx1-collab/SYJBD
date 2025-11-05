using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SYJBD.Data;
using SYJBD.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SYJBD.Services
{
    public class ProductosService : IProductosService
    {
        private readonly ErpDbContext _db;
        public ProductosService(ErpDbContext db) => _db = db;

        // Helpers
        private static async Task EnsureOpenAsync(MySqlConnection cn, CancellationToken ct = default)
        {
            if (cn.State != ConnectionState.Open) await cn.OpenAsync(ct);
        }
        private static string? S(MySqlDataReader rd, int i) => rd.IsDBNull(i) ? null : rd.GetString(i);
        private static decimal D(MySqlDataReader rd, int i) => rd.IsDBNull(i) ? 0m : rd.GetDecimal(i);

        // 1) Listado paginado
        public async Task<PagedResult<Producto>> ListarAsync(string? q, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _db.Productos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();

                if (int.TryParse(term, out var id))
                {
                    // por ID: comienza con
                    query = query.Where(p => p.IdProducto.ToString().StartsWith(term));
                }
                else
                {
                    // por nombre: todas las palabras en cualquier orden
                    var tokens = term
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .ToArray();

                    if (tokens.Length > 0)
                    {
                        foreach (var t in tokens)
                        {
                            var tt = t; // evita cierre sobre variable del foreach
                            query = query.Where(p => p.Nombre != null &&
                                                     EF.Functions.Like(p.Nombre, "%" + tt + "%"));
                        }
                    }
                }
            }


            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(p => p.IdProducto)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Producto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Query = q ?? ""
            };
        }

        // 2) Tallas + costo + precio (orden fijo)
        public async Task<IReadOnlyList<ProductoTallaPrecioVM>> GetTallasPreciosAsync(int idProducto, CancellationToken ct = default)
        {
            var list = new List<ProductoTallaPrecioVM>();
            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            const string ORDER_TALLAS =
                "'T-0','T-1','T-2','T-4','T-6','T-8','T-10','T-12','T-14','T-16'," +
                "'T-S','T-M','T-L','T-XL','T-XXL','T-GRD','T-MED','T-PEQ','T-STD'";

            using var cmd = cn.CreateCommand();
            cmd.CommandText = $@"
SELECT pt.id_producto, pt.id_talla, pt.id_prod_talla, COALESCE(pt.costo,0), COALESCE(pt.precio,0)
FROM vta_producto_talla pt
WHERE pt.id_producto = @id
ORDER BY
  IF(FIELD(pt.id_talla,{ORDER_TALLAS})=0,999,FIELD(pt.id_talla,{ORDER_TALLAS})),
  pt.id_talla;";
            cmd.Parameters.AddWithValue("@id", idProducto);

            using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                list.Add(new ProductoTallaPrecioVM
                {
                    IdProducto = rd.GetInt32(0),
                    IdTalla = S(rd, 1) ?? "",
                    IdProductoTalla = S(rd, 2) ?? "",
                    Costo = D(rd, 3),
                    Precio = D(rd, 4)
                });
            }

            return list;
        }

        public async Task<ProductoTallaEtiquetaVM?> GetProductoTallaAsync(int idProducto, string idProductoTalla, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idProductoTalla))
            {
                return null;
            }

            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
SELECT p.id_producto, COALESCE(p.nombre,''), pt.id_talla, pt.id_prod_talla
FROM vta_producto p
INNER JOIN vta_producto_talla pt ON pt.id_producto = p.id_producto
WHERE p.id_producto = @id AND pt.id_prod_talla = @ipt
LIMIT 1;";
            cmd.Parameters.AddWithValue("@id", idProducto);
            cmd.Parameters.AddWithValue("@ipt", idProductoTalla.Trim());

            using var rd = await cmd.ExecuteReaderAsync(ct);
            if (await rd.ReadAsync(ct))
            {
                return new ProductoTallaEtiquetaVM
                {
                    IdProducto = rd.GetInt32(0),
                    Nombre = S(rd, 1) ?? string.Empty,
                    IdTalla = S(rd, 2) ?? string.Empty,
                    IdProductoTalla = S(rd, 3) ?? string.Empty
                };
            }

            return null;
        }

        // 3) Actualizar SOLO el precio
        public async Task<(bool Ok, string Msg)> ActualizarPrecioAsync(int idProducto, string idTalla, decimal precio, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(idTalla)) return (false, "Talla inválida.");
            if (precio < 1m) return (false, "El precio mínimo es 1.00.");

            var cn = (MySqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(cn, ct);

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
UPDATE vta_producto_talla
   SET precio = @p
 WHERE id_producto = @id AND id_talla = @t;";
            cmd.Parameters.AddWithValue("@p", Math.Round(precio, 2));
            cmd.Parameters.AddWithValue("@id", idProducto);
            cmd.Parameters.AddWithValue("@t", idTalla.Trim());

            var n = await cmd.ExecuteNonQueryAsync(ct);
            return (n > 0, n > 0 ? "Precio actualizado." : "No se encontró la talla del producto.");
        }
    }
}
