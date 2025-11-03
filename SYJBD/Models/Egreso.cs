using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("T_Egresos")]
    public class Egreso
    {
        [Key]
        [Column("id_egreso"), StringLength(10)]
        public string IdEgreso { get; set; } = null!;

        [Column("id_tipo_egreso"), StringLength(10)]
        public string? IdTipoEgreso { get; set; }

        [Column("id_usuario"), StringLength(10)]
        public string? IdUsuario { get; set; }

        [Column("id_caja")]
        public int? IdCaja { get; set; }

        [Column("id_tienda"), StringLength(10)]
        public string? IdTienda { get; set; }

        [Column("detalle", TypeName = "text")]
        public string? Detalle { get; set; }

        [Column("monto", TypeName = "decimal(10,2)")]
        public decimal? Monto { get; set; }

        [Column("metodo_pago"), StringLength(30)]
        public string? MetodoPago { get; set; }

        [Column("estado"), StringLength(20)]
        public string? Estado { get; set; }

        [Column("fecha_registro")]
        public DateTime? FechaRegistro { get; set; }
    }
}
