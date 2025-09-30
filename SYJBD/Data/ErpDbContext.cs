using Microsoft.EntityFrameworkCore;
using SYJBD.Models;

namespace SYJBD.Data
{
    /// <summary>
    /// DbContext de la app. Por ahora usas CRUD de Unidades y lista de Productos,
    /// así que dejamos mapeos para esas entidades. Si más adelante retomas Cajas/POS,
    /// puedes reactivar sus DbSet y configuraciones.
    /// </summary>
    public class ErpDbContext : DbContext
    {
        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

        // --- Entidades que hoy usas en producción ---
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Unidad> Unidades => Set<Unidad>();

        // --- (Opcional futuro) Si mantienes los modelos en el proyecto,
        //     puedes habilitar estos DbSet cuando vuelvas a trabajar Cajas/POS.
        // public DbSet<Caja>    Cajas    => Set<Caja>();
        // public DbSet<Venta>   Ventas   => Set<Venta>();
        // public DbSet<Egreso>  Egresos  => Set<Egreso>();
        // public DbSet<Kardex>  Kardex   => Set<Kardex>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ------------------------------------------------------------
            // Producto
            // ------------------------------------------------------------
            // Si tu clase Producto ya tiene [Table]/[Column], no hace falta más.
            // De lo contrario, aquí puedes ajustar nombres y tipos.
            // Ejemplo (descomenta y adapta si lo necesitas):
            // mb.Entity<Producto>(e =>
            // {
            //     e.ToTable("vta_producto");
            //     e.HasKey(p => p.IdProducto);
            //     e.Property(p => p.IdProducto).HasColumnName("id_producto");
            //     e.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(200);
            //     // ...otros campos...
            // });

            // ------------------------------------------------------------
            // Unidad  (este sí lo mapeamos explícito porque ya lo usas en CRUD)
            // ------------------------------------------------------------
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

            // ------------------------------------------------------------
            // Si más adelante vuelves a Cajas/POS, aquí puedes setear
            // precisión de decimales y controlar fechas si hace falta.
            // ------------------------------------------------------------
            // mb.Entity<Caja>(e =>
            // {
            //     e.Property(p => p.MontoApertura).HasColumnType("decimal(12,2)");
            //     e.Property(p => p.MontoCierre).HasColumnType("decimal(12,2)");
            //     // ...
            // });
            // mb.Entity<Venta>(e =>
            // {
            //     e.Property(p => p.Total).HasColumnType("decimal(12,2)");
            //     // ...
            // });
        }
    }
}
