using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ezel_Market.Models;
using Microsoft.AspNetCore.Identity;

namespace Ezel_Market.Data
{

    public class ApplicationDbContext : IdentityDbContext<Usuarios>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //PRODUCTOS Y CATEGORIAS JALA
        public DbSet<Productos> Productos { get; set; }
        public DbSet<Categorias> Categoria { get; set; }

        //FIN PRODUCTOS Y CATEGORIAS JALA

        // 👇 Agrega esta línea (es lo nuevo)
        public DbSet<Inventario> Inventarios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Usuarios>().ToTable("t_usuario");

            // SEED DATA PARA para Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "2c040acd-d2fb-43ef-5fc5-c2e3f886ff01",
                    Name = "Cliente",
                    NormalizedName = "CLIENTE",
                    ConcurrencyStamp = "f7a8b9c0-d1e2-4f5a-8b7c-9d0e1f2a3b4c"
                },
                new IdentityRole
                {
                    Id = "3a8e1fdb-7c2d-4a5e-8f1c-9d3b2a1edf5c",
                    Name = "Administrador",
                    NormalizedName = "ADMINISTRADOR",
                    ConcurrencyStamp = "e6d5c4b3-a2b1-4c8d-9e0f-1a2b3c4d5e6f"
                }
            );

                    builder.Entity<Categorias>().HasData(
            new Categorias { Id = 1, Nombre = "Cervezas", Descripcion = "Bebidas a base de cebada fermentada" },
            new Categorias { Id = 2, Nombre = "Vinos", Descripcion = "Bebidas a base de uva fermentada" },
            new Categorias { Id = 3, Nombre = "Piscos", Descripcion = "Destilados de uva" },
            new Categorias { Id = 4, Nombre = "Rones", Descripcion = "Destilados de caña de azúcar" },
            new Categorias { Id = 5, Nombre = "Whisky", Descripcion = "Destilados de grano envejecidos en madera" },
            new Categorias { Id = 6, Nombre = "Tequila", Descripcion = "Destilados de agave azul" },
            new Categorias { Id = 7, Nombre = "Vodka y Gin", Descripcion = "Destilados blancos y ginebras" },
            new Categorias { Id = 8, Nombre = "Complementos", Descripcion = "Mezcladores, gaseosas y otros" }
        );

        }
    }
}
