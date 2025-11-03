using System;

namespace SYJBD.Models.Ventas
{
    public class VentaFiltroVM
    {
        // Fechas (visibles)
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }

        // Filtros por etiqueta (el usuario ve nombres, no IDs)
        public string? Tienda { get; set; }     // "TODOS" | "CERCADO 762" | ...
        public string? Vendedor { get; set; }   // "TODOS" | "MARIA"       | ...
        public string? TipoDoc { get; set; }    // "TODOS" | "NOTA DE VENTA" | ...
        public string? Cliente { get; set; }    // "TODOS" | "CLIENTE GENERICO" | ...
        public string? Estado { get; set; }     // "TODOS" | "ATENDIDO" | "ANULADO" | ...
        public string? Serie { get; set; }      // "TODOS" | "NT001" | "TK001" | ... (opcional)

        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int Offset => (Page <= 1 ? 0 : (Page - 1) * PageSize);

        // Limpia a null si viene "TODOS" o vacío
        public static string? Normalize(string? label)
        {
            if (string.IsNullOrWhiteSpace(label)) return null;
            var u = label.Trim().ToUpperInvariant();
            return u == "TODOS" ? null : label.Trim();
        }
    }
}
