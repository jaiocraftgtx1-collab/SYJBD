namespace SYJBD.Models.Ventas
{
    public class VentaItemVM
    {
        public int IdVenta { get; set; }              // <- NUEVO
        public string NumeroDoc { get; set; } = "";   // <- NUEVO
        public string FechaVenta { get; set; } = "";
        public string Serie { get; set; } = "";
        public int Nro { get; set; }
        public decimal Monto { get; set; }
        public string Vendedor { get; set; } = "";
        public string Cliente { get; set; } = "";
        public int Caja { get; set; }
        public string Tienda { get; set; } = "";
        public string Estado { get; set; } = "";

        public int IdProducto { get; set; }
        public string IdTalla { get; set; } = "";
        public string IdUnidad { get; set; } = "UND";
        public decimal Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal Importe => Math.Round(Cantidad * PrecioUnit, 2);
        // Para orden por bloque/talla si lo necesitas
        public string? Ord { get; set; }
        public string? NombreProducto { get; set; } // solo visual

    }
}
