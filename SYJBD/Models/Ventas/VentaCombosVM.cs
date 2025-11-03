using System.Collections.Generic;

namespace SYJBD.Models.Ventas
{
    public class VentaCombosVM
    {
        public IEnumerable<string> Tiendas { get; set; } = new List<string>();
        public IEnumerable<string> Vendedores { get; set; } = new List<string>();
        public IEnumerable<string> TiposDoc { get; set; } = new List<string>();
        public IEnumerable<string> Clientes { get; set; } = new List<string>();
        public IEnumerable<string> Estados { get; set; } = new List<string>();
        public IEnumerable<string> Series { get; set; } = new List<string>(); // opcional

        public string IdTienda { get; set; } = "";
        public int IdCaja { get; set; }
        public string IdUsuario { get; set; } = "";
        public string SerieNTV { get; set; } = "NV";   // muestra
        public string SerieBLT { get; set; } = "B001";
        public string SerieFCT { get; set; } = "F001";
        // Cliente genérico (si aplica)
        public string IdClienteGenerico { get; set; } = "SYJ0007";
        public string RucGenerico { get; set; } = "77777777";
        public string NombreGenerico { get; set; } = "CLIENTE SYJ STYLE";
    }
}
