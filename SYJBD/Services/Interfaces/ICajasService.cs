using SYJBD.Models;
using SYJBD.POS.ViewModels.Cajas;
using SYJBD.ViewModels.Cajas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SYJBD.Services.Interfaces
{
    public interface ICajasService
    {
        Task<PagedResult<CajaRowVM>> GetCajasAsync(int page, int pageSize, string? search);
        Task<Caja?> GetCajaAsync(int idCaja);
        Task<CajaReporteVM?> GetCabeceraReporteAsync(int idCaja);
        Task<IReadOnlyList<(string Metodo, double Monto)>> GetMontosPorMetodoPagoAsync(int idCaja);
        Task<IReadOnlyList<Egreso>> GetEgresosCajaAsync(int idCaja, string idUsuario);
        Task<IReadOnlyList<TopProductoVM>> GetTopProductosAsync(int idCaja, int take = 50);
        Task<(int atendidas, int anuladas)> GetContadorVentasAsync(int idCaja);
        Task<(int aut, int pen, int anu)> GetContadorEgresosAsync(int idCaja);
        Task<bool> PuedeContinuarAsync(int idCaja);
        Task<bool> PuedeCerrarAsync(int idCaja);
        Task<bool> CerrarCajaAsync(int idCaja, string idUsuarioCierre, decimal montoCierre, string observacion);
    }
}
