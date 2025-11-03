// Services/IVentaService.cs
using SVJBD.Models.Ventas;
using SYJBD.Models;
using SYJBD.Models.Ventas;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SYJBD.Services
{
    public interface IVentaService
    {
        // Listado de ventas (vista Index)
        Task<VentaCombosVM> GetCombosAsync();
        Task<(PagedResult<VentaItemVM> Paged, decimal TotalMonto)> ListarAsync(VentaFiltroVM filtro);
        Task<VentaComprobanteVM?> GetComprobanteAsync(int idVenta);

        // Nueva venta
        Task<VentaCombosVM> GetCombosNuevaVentaAsync(string idTienda, string idUsuario, int idCaja, CancellationToken ct = default);
        Task<IEnumerable<dynamic>> BuscarClientesAsync(string query, CancellationToken ct = default);
        Task<IEnumerable<dynamic>> BuscarProductosAsync(string query, CancellationToken ct = default);
        Task<int> TomarCorrelativoAsync(string tipoDocumento, CancellationToken ct = default);
        Task<int> RegistrarVentaAsync(VentaCreateVM vm, CancellationToken ct = default);
        // NUEVOS PARA ANULAR
        Task<AnularVentaPreviewVM?> GetAnularPreviewAsync(int idVenta, CancellationToken ct = default);
        Task<(bool Ok, string Msg)> AnularVentaAsync(int idVenta, CancellationToken ct = default);

        // === PUNTO DE VENTA / CAJAS ===
        Task<int> GetNextIdCajaAsync(CancellationToken ct = default);
        Task<IEnumerable<Tienda>> ListarTiendasAsync(CancellationToken ct = default);

        /// <summary>
        /// Abre una caja para la tienda indicada. Regla: solo 1 caja abierta por tienda en el día.
        /// </summary>
        Task<(bool Ok, string Msg, int IdCaja)> AbrirCajaAsync(
            string idTienda,
            string idUsuarioApertura,
            decimal montoApertura,
            CancellationToken ct = default);
        Task<CajaCerrarVM> GetDatosCierreAsync(int idCaja, CancellationToken ct = default);
        Task<(bool Ok, string Msg)> CerrarCajaAsync(int idCaja, string idUsuarioCierre, decimal efectivoReal, string observacion, CancellationToken ct = default);
    }
}
