using SYJBD.POS.ViewModels.Cajas;
using SYJBD.ViewModels.Cajas;
using System.Threading.Tasks;

namespace SYJBD.Services.Interfaces
{
    public interface IReporteCajaService
    {
        Task<CajaReporteVM?> BuildAsync(int idCaja, string idUsuarioActual);
    }
}
