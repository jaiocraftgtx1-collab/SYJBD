using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using SYJBD.Models;

namespace SYJBD.Data
{
    public class ErpDbContext : DbContext
    {
        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Unidad> Unidades => Set<Unidad>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ----- Producto -----
            mb.Entity<Producto>().HasKey(p => p.IdProducto);

            // ----- Unidad (vta_unidad) -----
            mb.Entity<Unidad>(e =>
            {
                e.ToTable("vta_unidad");
                e.HasKey(u => u.IdUnidad);

                e.Property(u => u.IdUnidad)
                 .HasColumnName("id_unidad")
                 .HasMaxLength(10)
                 .IsRequired();

                e.Property(u => u.Nombre)
                 .HasColumnName("nombre")
                 .HasMaxLength(50)
                 .IsRequired();
            });
        }
    }
}
