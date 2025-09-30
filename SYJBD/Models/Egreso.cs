using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("T_Egresos")]
    public class Egreso
    {
        // En tu VBA lo tratas como texto (se muestra “EG000…”),
        // por eso dejo string; si es INT en tu BD, cambia a int.
        [Key]
        [Column("id_egreso")]
        [StringLength(20)]
        public string IdEgreso { get; set; } = "";

        [Required]
        [Column("id_caja")]
        public int IdCaja { get; set; }

        [Column("id_usuario", TypeName = "varchar(10)")]
        public string? IdUsuario { get; set; }

        [Column("id_tipo_egreso")]
        public int? IdTipoEgreso { get; set; }

        [Required]
        [Column("detalle", TypeName = "varchar(255)")]
        public string Detalle { get; set; } = "";

        [Required]
        [Column("monto", TypeName = "decimal(12,2)")]
        public decimal Monto { get; set; }

        [Column("metodo_pago", TypeName = "varchar(30)")]
        public string? MetodoPago { get; set; }

        [Column("fecha_registro")]
        public DateTime? FechaRegistro { get; set; }

        // Estados usados: PENDIENTE / AUTORIZADO / ANULADO
        [Required]
        [Column("estado", TypeName = "varchar(20)")]
        public string Estado { get; set; } = "PENDIENTE";
    }
}
