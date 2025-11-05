using SYJBD.Services;

namespace SYJBD.Models
{
    public sealed class BarcodePrintPageVM
    {
        public ProductoTallaEtiquetaVM? Detalle { get; set; }
            = null;

        public BarcodeLayoutDefinition? Layout { get; set; }
            = null;

        public int Copias { get; set; }
            = 1;

        public string? ImpresoraId { get; set; }
            = null;

        public string ImpresoraNombre { get; set; }
            = string.Empty;

        public string? ErrorMessage { get; set; }
            = null;
    }
}
