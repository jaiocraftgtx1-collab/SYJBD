using System.Collections.Generic;

namespace SYJBD.Services
{
    public sealed class BarcodeLayoutOptions
    {
        public IReadOnlyList<BarcodeLayoutDefinition> Layouts { get; set; }
            = new List<BarcodeLayoutDefinition>();

        public IReadOnlyList<BarcodePrinterOption> Printers { get; set; }
            = new List<BarcodePrinterOption>();
    }

    public sealed class BarcodeLayoutDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public double WidthMm { get; set; }
            = 38;
        public double HeightMm { get; set; }
            = 25;
        public int Columns { get; set; }
            = 1;
        public int Rows { get; set; }
            = 1;
        public double HorizontalGapMm { get; set; }
            = 0;
        public double VerticalGapMm { get; set; }
            = 0;
        public string? Description { get; set; }
            = null;
    }

    public sealed class BarcodePrinterOption
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
