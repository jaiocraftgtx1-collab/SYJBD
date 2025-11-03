namespace SYJBD.Models
{
    public class CajaListadoVM
    {
        public int IdCaja { get; set; }

        public string IdTienda { get; set; } = "";

        public string IdUsuarioApertura { get; set; } = "";

        public DateTime FechaApertura { get; set; }

        public string? IdUsuarioCierre { get; set; }

        public DateTime? FechaCierre { get; set; }

        // <— esta faltaba
        public string Observacion { get; set; } = "";
    }
}
