namespace SYJBD.Models.Ventas
{
    public class VentaListaVM
    {
        public PagedResult<VentaItemVM> Paged { get; set; } = new PagedResult<VentaItemVM>();
        public VentaFiltroVM Filtro { get; set; } = new VentaFiltroVM();
        public VentaCombosVM Combos { get; set; } = new VentaCombosVM();

        // Totales de la grilla
        public decimal TotalMonto { get; set; }
    }
}
