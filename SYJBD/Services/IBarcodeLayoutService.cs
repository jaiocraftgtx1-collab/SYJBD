using System.Collections.Generic;

namespace SYJBD.Services
{
    public interface IBarcodeLayoutService
    {
        IReadOnlyList<BarcodeLayoutDefinition> GetLayouts();
        IReadOnlyList<BarcodePrinterOption> GetPrinters();
        BarcodeLayoutDefinition? FindLayout(string? id);
    }
}
