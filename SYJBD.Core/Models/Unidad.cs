using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("vta_unidad")]
    public class Unidad
    {
        [Key]
        [Column("id_unidad")]
        [StringLength(10)]
        [Required]
        public string IdUnidad { get; set; } = string.Empty;

        [Column("nombre")]
        [StringLength(50)]
        [Required]
        public string Nombre { get; set; } = string.Empty;
    }
}
