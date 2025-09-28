using Microsoft.EntityFrameworkCore;
using SYJBD.Models;
using SYJBD.Models;

namespace SYJBD.Data
{
    public class ErpDbContext : DbContext
    {
        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

        public DbSet<Producto> Productos => Set<Producto>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Producto>().HasKey(p => p.IdProducto);
        }
    }
}
