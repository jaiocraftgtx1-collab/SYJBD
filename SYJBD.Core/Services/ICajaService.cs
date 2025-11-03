using System.Threading.Tasks;
using SYJBD.Models;

namespace SYJBD.Services
{
    public interface ICajaService
    {
        Task<PagedResult<CajaListadoVM>> ListarAsync(int page, int pageSize);
        Task<CajaReporteVM> GetReporteAsync(int idCaja, string? rolUsuario);

        Task<int> GetNextIdCajaAsync(CancellationToken ct = default);
        Task<IEnumerable<Tienda>> ListarTiendasAsync(CancellationToken ct = default);

        Task<(bool Ok, string Msg, int IdCaja)> AbrirCajaAsync(
            string idTienda, string idUsuarioApertura, decimal montoApertura,
            CancellationToken ct = default);
    }
}
