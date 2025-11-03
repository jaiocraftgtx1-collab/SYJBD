using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("vta_tienda")]
    public class Tienda
    {
        [Key]
        [Column("id_tienda"), StringLength(10)]
        public string IdTienda { get; set; } = null!;

        [Column("nombre"), StringLength(100)]
        public string Nombre { get; set; } = null!;

        [Column("ubicacion"), StringLength(200)]
        public string Ubicacion { get; set; } = null!;
    }
}
