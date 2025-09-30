using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("vta_venta")]
    public class Venta
    {
        [Key]
        [Column("id_venta")]
        public long IdVenta { get; set; }

        [Required]
        [Column("id_caja")]
        public int IdCaja { get; set; }

        [Column("id_tienda", TypeName = "varchar(10)")]
        public string? IdTienda { get; set; }

        [Column("id_usuario", TypeName = "varchar(10)")]
        public string? IdUsuario { get; set; }

        [Column("fecha_registro")]
        public DateTime? FechaRegistro { get; set; }

        // ACTIVO / ANULADO
        [Required]
        [Column("estado", TypeName = "varchar(20)")]
        public string Estado { get; set; } = "ACTIVO";

        // ATENDIDO / ANULADO (lógico de negocio)
        [Required]
        [Column("estado_venta", TypeName = "varchar(20)")]
        public string EstadoVenta { get; set; } = "ATENDIDO";

        // Métodos de pago 1 y 2 (según tu VBA)
        [Column("mp1", TypeName = "varchar(30)")]
        public string? Mp1 { get; set; }

        [Column("monto_mp1", TypeName = "decimal(12,2)")]
        public decimal? MontoMp1 { get; set; }

        [Column("mp2", TypeName = "varchar(30)")]
        public string? Mp2 { get; set; }

        [Column("monto_mp2", TypeName = "decimal(12,2)")]
        public decimal? MontoMp2 { get; set; }

        // Totales opcionales
        [Column("total", TypeName = "decimal(12,2)")]
        public decimal? Total { get; set; }
    }
}
