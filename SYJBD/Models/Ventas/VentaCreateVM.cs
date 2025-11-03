using System;
using System.Collections.Generic;

namespace SYJBD.Models.Ventas
{
    /// <summary>
    /// DTO que la pantalla "NuevaVenta" enviará al backend.
    /// Mantén estos nombres: son los que usaremos en el Service.
    /// </summary>
    public class VentaCreateVM
    {
        // Cabecera mínima
        public int IdCaja { get; set; }
        public string IdTienda { get; set; } = "";
        public string IdUsuario { get; set; } = "";
        public string IdCliente { get; set; } = "";
        public string TipoDocumento { get; set; } = "";  // NTV/TKT/BLT/FCT
        public string Serie { get; set; } = "";          // NV / B001 / F001, etc.
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        // Métodos de pago
        public string? MP1 { get; set; }
        public decimal MontoMP1 { get; set; }
        public string? MP2 { get; set; }
        public decimal MontoMP2 { get; set; }

        // Total
        public decimal Total { get; set; }

        // Detalle
        public List<VentaCreateItemVM> Items { get; set; } = new();
    }

    public class VentaCreateItemVM
    {
        public int IdProducto { get; set; }
        public string IdTalla { get; set; } = "";
        public string IdUnidad { get; set; } = "UND";
        public decimal Cantidad { get; set; }      // se guarda como decimal(10,2)
        public decimal PrecioUnit { get; set; }    // decimal(10,2)
    }
}
