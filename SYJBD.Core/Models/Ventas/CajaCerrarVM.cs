using System;

namespace SVJBD.Models.Ventas
{
    public class CajaCerrarVM
    {
        // ===== Datos de lectura para el modal =====
        public int IdCaja { get; set; }
        public string IdTienda { get; set; } = "";
        public string IdUsuarioApertura { get; set; } = "";
        public DateTime FechaApertura { get; set; }
        public decimal MontoApertura { get; set; }
        public decimal TotalIngresosEfectivo { get; set; }
        public decimal TotalGastosEfectivoAut { get; set; }
        public int EgresosPendientes { get; set; }
        public decimal EfectivoEsperado { get; set; }

        // ===== Datos de entrada (POST) =====
        public string IdUsuarioCierre { get; set; } = "";
        public decimal EfectivoReal { get; set; }
        public string Observacion { get; set; } = "";
        public string? EfectivoReal2 { get; set; }   // viene del <input name="EfectivoReal2">

    }
}
