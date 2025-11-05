using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace SYJBD.Services
{
    public sealed class BarcodeLayoutService : IBarcodeLayoutService
    {
        private readonly BarcodeLayoutOptions _options;

        public BarcodeLayoutService(IOptions<BarcodeLayoutOptions> options)
        {
            _options = options.Value ?? new BarcodeLayoutOptions();
        }

        public IReadOnlyList<BarcodeLayoutDefinition> GetLayouts() =>
            _options.Layouts ?? Array.Empty<BarcodeLayoutDefinition>();

        public IReadOnlyList<BarcodePrinterOption> GetPrinters() =>
            _options.Printers ?? Array.Empty<BarcodePrinterOption>();

        public BarcodeLayoutDefinition? FindLayout(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return (_options.Layouts ?? Array.Empty<BarcodeLayoutDefinition>())
                .FirstOrDefault(l => string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
