using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SYJBD.Models;
namespace SYJBD.Services
{
    public interface IProductosService
    {
        Task<PagedResult<Producto>> ListarAsync(
            string? q, int page = 1, int pageSize = 10, CancellationToken ct = default);

        Task<IReadOnlyList<ProductoTallaPrecioVM>> GetTallasPreciosAsync(
            int idProducto, CancellationToken ct = default);

        Task<(bool Ok, string Msg)> ActualizarPrecioAsync(
            int idProducto, string idTalla, decimal precio, CancellationToken ct = default);
    }

    // ====== VMs ======
    public sealed class ProductoVM
    {
        public int IdProducto { get; set; }
        public string? Nombre { get; set; }
        public string? IdTipoProd { get; set; }
        public string? Estado { get; set; }
    }

    public sealed class ProductoTallaPrecioVM
    {
        public int IdProducto { get; set; }
        public string IdTalla { get; set; } = "";
        public decimal Costo { get; set; }
        public decimal Precio { get; set; }
    }

}
