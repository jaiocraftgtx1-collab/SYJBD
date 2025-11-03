using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("T_TipoEgreso")]
    public class TipoEgreso
    {
        [Key]
        [Column("id_tipo_egreso"), StringLength(10)]
        public string IdTipoEgreso { get; set; } = null!;

        // Ej.: OPERATIVO / ADMINISTRATIVO
        [Column("tipo"), StringLength(30)]
        public string Tipo { get; set; } = null!;
    }
}
