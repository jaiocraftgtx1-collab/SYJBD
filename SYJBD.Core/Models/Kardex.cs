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
        public int IdKardex { get; set; }

        [Column("tipo_movimiento"), StringLength(3)]
        public string TipoMovimiento { get; set; } = null!;   // SV / EN / etc.

        [Column("tipo_ref"), StringLength(30)]
        public string? TipoRef { get; set; }

        [Column("doc_ref"), StringLength(30)]
        public string? DocRef { get; set; }

        [Column("nro_doc"), StringLength(20)]
        public string? NroDoc { get; set; }

        [Column("id_origen")]
        public int? IdOrigen { get; set; }

        [Column("fecha_movimiento")]
        public DateTime FechaMovimiento { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("id_talla"), StringLength(10)]
        public string IdTalla { get; set; } = null!;

        [Column("id_unidad"), StringLength(10)]
        public string? IdUnidad { get; set; } = "UND";

        [Column("cantidad_movida", TypeName = "decimal(10,2)")]
        public decimal CantidadMovida { get; set; }

        [Column("costo_unitario", TypeName = "decimal(10,2)")]
        public decimal? CostoUnitario { get; set; }

        [Column("costo_total", TypeName = "decimal(12,2)")]
        public decimal? CostoTotal { get; set; }

        [Column("moneda"), StringLength(10)]
        public string? Moneda { get; set; } = "SOLES";

        [Column("tipo_cambio", TypeName = "decimal(10,4)")]
        public decimal? TipoCambio { get; set; } = 1.0000m;

        [Column("id_usuario"), StringLength(10)]
        public string IdUsuario { get; set; } = null!;

        [Column("estado"), StringLength(10)]
        public string? Estado { get; set; } = "ACTIVO";
    }
}
