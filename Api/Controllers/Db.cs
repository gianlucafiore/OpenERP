using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Protocols;
using Pomelo.EntityFrameworkCore.MySql;

namespace Api
{
    public class Db : DbContext
    {
        public DbSet<Acount> Acount{get;set;}
        public DbSet<Venta> Venta{get;set;}
        public DbSet<ItemVenta> ItemVenta{get;set;}
        public DbSet<Producto> Producto{get;set;}
        public DbSet<Stock> Stock{get;set;}

        public DbSet<Cliente> Cliente{get;set;}
        public DbSet<Pago> Pago{get;set;}

        public string ConnStr{get;set;}
        public Db(DbContextOptions<Db> options) : base(options){}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Acount>(entity =>
            {
                entity.HasKey(e => e.IdAcount);
            });
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(v => v.IdVenta);
            });
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.IdProducto);
            });
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasKey(p => p.IdStock);
            });
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(c => c.IdCliente);
            });
            modelBuilder.Entity<ItemVenta>(entity =>
            {
                entity.HasKey(i => i.IdItemVenta);
            });
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(p => p.IdPago);
            });
        }
    }
}