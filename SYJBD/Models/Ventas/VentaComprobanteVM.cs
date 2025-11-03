using System;
using System.Collections.Generic;

namespace SYJBD.Models.Ventas
{
    public class VentaComprobanteVM
    {
        // Cabecera
        public int IdVenta { get; set; }
        public string TipoDocumento { get; set; } = "";
        public string TipoDocumentoLabel => string.IsNullOrWhiteSpace(TipoDocumento) ? "COMPROBANTE" : TipoDocumento;
        public string Serie { get; set; } = "";
        public DateTime FechaVenta { get; set; }

        public int Correlativo { get; set; }                  // ya lo estás llenando en el service
        public string NumeroDoc => $"{Serie}-{Correlativo}";  // alias listo para mostrar



        public string EstadoVenta { get; set; } = "";
        public string Estado { get; set; } = "";

        public int IdCaja { get; set; }
        public string IdTienda { get; set; } = "";
        public string TiendaNombre { get; set; } = "";

        public string IdCliente { get; set; } = "";
        public string ClienteNombre { get; set; } = "";
        public string ClienteRuc { get; set; } = "";

        public string IdUsuario { get; set; } = "";
        public string VendedorNombre { get; set; } = "";   // usado en TicketA4/Ticket80

        // Pagos
        public string? MP1 { get; set; }
        public decimal MontoMP1 { get; set; }
        public string? MP2 { get; set; }
        public decimal MontoMP2 { get; set; }

        // Totales
        public decimal Total { get; set; }
        public decimal IgvTasa { get; set; } = 0.18m;
        public decimal Igv { get; set; }
        public decimal SubTotal { get; set; }

        // Detalle
        public List<VentaItemDetalleVM> Items { get; set; } = new();

        // Extras de impresión
        public string Moneda { get; set; } = "SOLES";
        public bool IsPdf { get; set; } = false;     // true cuando generas PDF
    }

    public class VentaItemDetalleVM
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public string IdTalla { get; set; } = "";
        public string IdUnidad { get; set; } = "";
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
    }
}
