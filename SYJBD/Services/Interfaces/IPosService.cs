using System.Threading.Tasks;

namespace SYJBD.Services.Interfaces
{
    public interface IPosService
    {
        Task<bool> ValidarCajaAbiertaAsync(int idCaja);
    }
}
