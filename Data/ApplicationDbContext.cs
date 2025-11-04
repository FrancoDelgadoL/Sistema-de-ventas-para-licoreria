using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ezel_Market.Models;
using Microsoft.AspNetCore.Identity;

namespace Ezel_Market.Data;

public class ApplicationDbContext : IdentityDbContext<Usuarios>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
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
            },
            new IdentityRole
            {
                Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                Name = "Gerente",
                NormalizedName = "GERENTE",
                ConcurrencyStamp = "c29b1a1f-8b3c-4d5e-9f6a-1b2c3d4e5f6a"
            },
            new IdentityRole
            {
                Id = "f47ac10b-58cc-4372-a567-0e02b2c3d479",
                Name = "Inventario",
                NormalizedName = "INVENTARIO",
                ConcurrencyStamp = "7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"
            }
        );
        
    }

    
            
}
