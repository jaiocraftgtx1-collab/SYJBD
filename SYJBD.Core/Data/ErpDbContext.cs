using Microsoft.EntityFrameworkCore;
using SYJBD.Models;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SYJBD.Data
{
    /// <summary>
    /// DbContext de la app.
    /// </summary>
    public class ErpDbContext : DbContext
    {
        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

        // Entidades en uso
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Unidad> Unidades => Set<Unidad>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<Kardex> Kardex => Set<Kardex>();
        public DbSet<Egreso> Egresos => Set<Egreso>();
        public DbSet<Tienda> Tiendas => Set<Tienda>();
        public DbSet<TipoEgreso> TipoEgresos => Set<TipoEgreso>();

        // Cajas (necesario para /Ventas/PuntoDeVenta)
        public DbSet<Caja> Cajas => Set<Caja>();
        public async Task EnsureOpenAsync(CancellationToken ct = default)
        {
            var cn = Database.GetDbConnection();
            if (cn.State != ConnectionState.Open)
                await cn.OpenAsync(ct);
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ------------------------------------------------------------
            // Producto
            // (Si tu clase Producto ya tiene DataAnnotations, no necesitas mapear aquí)
            // ------------------------------------------------------------
            // Ejemplo (descomenta si lo requieres):
            // mb.Entity<Producto>(e =>
            // {
            //     e.ToTable("vta_producto");
            //     e.HasKey(p => p.IdProducto);
            //     e.Property(p => p.IdProducto).HasColumnName("id_producto");
            //     e.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(200);
            //     // ...
            // });

            // ------------------------------------------------------------
            // Unidad (CRUD activo)
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
            // Caja (lectura para listado de punto de venta)
            // ------------------------------------------------------------
            mb.Entity<Caja>(e =>
            {
                e.ToTable("vta_caja");
                e.HasKey(x => x.IdCaja);

                e.Property(x => x.IdCaja).HasColumnName("id_caja");
                e.Property(x => x.IdTienda).HasColumnName("id_tienda");
                e.Property(x => x.IdUsuarioApertura).HasColumnName("id_usuario_apertura");
                e.Property(x => x.FechaApertura).HasColumnName("fecha_apertura");

                e.Property(x => x.MontoApertura).HasColumnName("monto_apertura");
                e.Property(x => x.IdUsuarioCierre).HasColumnName("id_usuario_cierre");
                e.Property(x => x.FechaCierre).HasColumnName("fecha_cierre");
                e.Property(x => x.MontoCierre).HasColumnName("monto_cierre");

                e.Property(x => x.TotalIngresos).HasColumnName("total_ingresos");
                e.Property(x => x.TotalGastos).HasColumnName("total_gastos");
                e.Property(x => x.EfectivoEsperado).HasColumnName("efectivo_esperado");
                e.Property(x => x.DiferenciaCaja).HasColumnName("diferencia_caja");
                e.Property(x => x.Observacion).HasColumnName("observacion");
            });
        }
    }
}

