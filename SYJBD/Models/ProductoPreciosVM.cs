namespace SYJBD.Models
{
    public class ProductoPreciosVM
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public List<ProductoPrecioRowVM> Tallas { get; set; } = new();
    }

    public class ProductoPrecioRowVM
    {
        public string IdTalla { get; set; } = "";
        public decimal Precio { get; set; }
    }

    public class PrecioUpdateVM
    {
        public int IdProducto { get; set; }
        public string IdTalla { get; set; } = "";
        public decimal Precio { get; set; }
    }
}
