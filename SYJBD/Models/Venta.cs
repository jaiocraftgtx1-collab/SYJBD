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
        public int IdVenta { get; set; }

        [Column("tipo_documento"), StringLength(5)]
        public string? TipoDocumento { get; set; }

        [Column("serie"), StringLength(10)]
        public string? Serie { get; set; }

        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; }

        [Column("id_cliente"), StringLength(10)]
        public string IdCliente { get; set; } = null!;

        [Column("id_usuario"), StringLength(10)]
        public string IdUsuario { get; set; } = null!;

        [Column("id_tienda"), StringLength(10)]
        public string IdTienda { get; set; } = null!;

        [Column("id_caja")]
        public int IdCaja { get; set; }

        [Column("mp1"), StringLength(20)]
        public string? MP1 { get; set; }

        [Column("monto_mp1", TypeName = "decimal(12,2)")]
        public decimal? MontoMP1 { get; set; }

        [Column("mp2"), StringLength(20)]
        public string? MP2 { get; set; }

        [Column("monto_mp2", TypeName = "decimal(12,2)")]
        public decimal? MontoMP2 { get; set; }

        [Column("total", TypeName = "decimal(12,2)")]
        public decimal? Total { get; set; }

        [Column("estado_venta"), StringLength(10)]
        public string? EstadoVenta { get; set; } = "ATENDIDO";

        [Column("estado"), StringLength(10)]
        public string? Estado { get; set; } = "ACTIVO";
    }
}
