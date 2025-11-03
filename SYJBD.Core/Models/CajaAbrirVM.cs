// Models/CajaAbrirVM.cs
using System.Collections.Generic;

namespace SYJBD.Models
{
    public class CajaAbrirVM
    {
        public string IdUsuario { get; set; } = "";
        public int NextIdCaja { get; set; }
        public IEnumerable<Tienda> Tiendas { get; set; } = new List<Tienda>();
    }
}
