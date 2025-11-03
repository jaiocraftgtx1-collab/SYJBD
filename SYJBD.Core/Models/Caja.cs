using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("vta_caja")]
    public class Caja
    {
        [Key]
        [Column("id_caja")]
        public int IdCaja { get; set; }

        [Required, StringLength(10)]
        [Column("id_tienda")]
        public string IdTienda { get; set; } = "";

        [Required, StringLength(10)]
        [Column("id_usuario_apertura")]
        public string IdUsuarioApertura { get; set; } = "";

        [Required]
        [Column("fecha_apertura")]
        public DateTime FechaApertura { get; set; }

        [Required]
        [Column("monto_apertura", TypeName = "decimal(12,2)")]
        public decimal MontoApertura { get; set; }

        [StringLength(10)]
        [Column("id_usuario_cierre")]
        public string? IdUsuarioCierre { get; set; }

        [Column("fecha_cierre")]
        public DateTime? FechaCierre { get; set; }

        [Column("monto_cierre", TypeName = "decimal(12,2)")]
        public decimal? MontoCierre { get; set; }

        [Column("total_ingresos", TypeName = "decimal(12,2)")]
        public decimal? TotalIngresos { get; set; }

        [Column("total_gastos", TypeName = "decimal(12,2)")]
        public decimal? TotalGastos { get; set; }

        [Column("efectivo_esperado", TypeName = "decimal(12,2)")]
        public decimal? EfectivoEsperado { get; set; }

        [Column("diferencia_caja", TypeName = "decimal(12,2)")]
        public decimal? DiferenciaCaja { get; set; }

        [Column("observacion", TypeName = "text")]
        public string? Observacion { get; set; }

        // Helpers no mapeados
        [NotMapped]
        public bool EstaAbierta => string.IsNullOrWhiteSpace(IdUsuarioCierre) || !FechaCierre.HasValue;

        [NotMapped]
        public string Estado =>
            string.IsNullOrWhiteSpace(Observacion)
                ? EstaAbierta ? "CAJA ABIERTA" : "—"
                : Observacion!.Trim();
    }
}
