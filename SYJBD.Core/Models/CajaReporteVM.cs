using System;
using System.Collections.Generic;

namespace SYJBD.Models
{
    public class CajaReporteVM
    {
        public CabeceraVM Cabecera { get; set; } = new();
        public List<MetodoPagoVM> MetodosPago { get; set; } = new();
        public List<EgresoRowVM> Egresos { get; set; } = new();
        public List<TopProductoVM> TopProductos { get; set; } = new();
        public ContadoresVM Contadores { get; set; } = new();

        // --------- Bloques ---------

        public class CabeceraVM
        {
            public int IdCaja { get; set; }
            public string IdTienda { get; set; } = "";
            public string TiendaNombre { get; set; } = "";
            public string IdUsuarioApertura { get; set; } = "";
            public DateTime FechaApertura { get; set; }
            public string? IdUsuarioCierre { get; set; }
            public DateTime? FechaCierre { get; set; }
            public string Observacion { get; set; } = "";
            public decimal? MontoApertura { get; set; }
            public decimal? MontoCierre { get; set; }
        }

        public class MetodoPagoVM
        {
            public string Metodo { get; set; } = "";
            public decimal Total { get; set; }
        }

        public class EgresoRowVM
        {
            public string Codigo { get; set; } = "";
            public string Detalle { get; set; } = "";
            public decimal Monto { get; set; }
            public string MetodoPago { get; set; } = "";
            public DateTime? Fecha { get; set; }
            public string Estado { get; set; } = "";
        }

        public class TopProductoVM
        {
            public int IdProducto { get; set; }
            public string Nombre { get; set; } = "";
            public string IdTalla { get; set; } = "";
            public int Cantidad { get; set; }
            public decimal Monto { get; set; }
        }

        public class ContadoresVM
        {
            public int VentasAtendidas { get; set; }
            public int VentasAnuladas { get; set; }
            public int EgresosAut { get; set; }
            public int EgresosPen { get; set; }
            public int EgresosAnu { get; set; }
        }
    }
}
