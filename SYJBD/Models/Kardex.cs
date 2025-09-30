using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SYJBD.Models
{
    [Table("vta_kardex")]
    public class Kardex
    {
        [Key]
        [Column("id_kardex")]
        public long IdKardex { get; set; }

        [Required]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        // id_origen = id_venta cuando tipo_movimiento = 'SV'
        [Required]
        [Column("id_origen")]
        public long IdOrigen { get; set; }

        [Column("id_talla", TypeName = "varchar(10)")]
        public string? IdTalla { get; set; }

        [Required]
        [Column("tipo_movimiento", TypeName = "varchar(5)")]
        public string TipoMovimiento { get; set; } = ""; // 'SV' para salida por venta

        [Required]
        [Column("cantidad_movida")]
        public decimal CantidadMovida { get; set; }

        [Required]
        [Column("costo_total", TypeName = "decimal(12,2)")]
        public decimal CostoTotal { get; set; }

        // ACTIVO / ANULADO
        [Required]
        [Column("estado", TypeName = "varchar(20)")]
        public string Estado { get; set; } = "ACTIVO";

        [Column("fecha_registro")]
        public DateTime? FechaRegistro { get; set; }
    }
}
