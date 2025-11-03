namespace SYJBD.Models.Ventas
{
    public class ProductoMiniDTO
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public string IdTalla { get; set; } = "";
        public string IdUnidad { get; set; } = "UND";
        public decimal PrecioUnit { get; set; }
    }
}
