using System.Collections.Generic;

namespace SYJBD.Models.Ventas
{
    public class NuevaVentaDTO
    {
        public string TipoDocumento { get; set; } = ""; // "NOTA DE VENTA" | "BOLETA ELECTRÓNICA" | "FACTURA ELECTRÓNICA"
        public string Serie { get; set; } = "";         // "NV" | "B001" | "F001"
        public int IdCaja { get; set; }
        public string IdTienda { get; set; } = "";
        public string IdUsuario { get; set; } = "";
        public string IdCliente { get; set; } = "";

        public List<NuevaVentaItemDTO> Items { get; set; } = new();
        public string? MP1 { get; set; }
        public decimal MontoMP1 { get; set; }
        public string? MP2 { get; set; }
        public decimal MontoMP2 { get; set; }
    }

    public class NuevaVentaItemDTO
    {
        public int IdProducto { get; set; }
        public string IdTalla { get; set; } = "";
        public string IdUnidad { get; set; } = "UND";
        public decimal Cantidad { get; set; }      // enteros (se validará)
        public decimal PrecioUnit { get; set; }    // decimal(10,2)
    }
}
