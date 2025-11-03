using System;

namespace SYJBD.Models.Ventas
{
    public class AnularVentaPreviewVM
    {
        public bool NotFound { get; set; }
        public int IdVenta { get; set; }
        public string NumeroDoc { get; set; } = "";
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string EstadoVenta { get; set; } = "";
        public string Estado { get; set; } = "";
        public int KardexSvActivos { get; set; }
    }
}
