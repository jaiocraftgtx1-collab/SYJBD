using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("vta_producto")]
    public class Producto
    {
        [Key]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Required, Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("id_tipo_prod")] public string IdTipoProd { get; set; } = string.Empty;
        [Column("id_categoria")] public string IdCategoria { get; set; } = string.Empty;
        [Column("id_unidad")] public string IdUnidad { get; set; } = string.Empty;
        [Column("fecha_crea")] public DateTime? FechaCrea { get; set; }
        [Column("id_subcategoria")] public string? IdSubcategoria { get; set; }
        [Column("origen")] public string? Origen { get; set; }
        [Column("estado")] public string? Estado { get; set; }
    }
}
