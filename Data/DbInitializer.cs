using Microsoft.AspNetCore.Identity;
using PuestoWeb.Models;

namespace PuestoWeb.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        context.Database.EnsureCreated();

        // 1. Crear Roles si no existen
        string[] roleNames = { "Admin", "Cliente" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Crear Usuario Admin por defecto
        var adminEmail = "admin@fruteria.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        // 3. Semilla de Artículos
        if (context.Articulos.Any())
        {
            return;   // DB has been seeded
        }

        var articulos = new Articulo[]
        {
            new Articulo { Nombre = "Manzana Roja", Descripcion = "Manzanas frescas y dulces de estación", Precio = 1500, Tipo = TipoArticulo.Fruta, ImagenUrl = "https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=500&auto=format" },
            new Articulo { Nombre = "Banana", Descripcion = "Bananas maduras de Ecuador", Precio = 1200, Tipo = TipoArticulo.Fruta, ImagenUrl = "https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?w=500&auto=format" },
            new Articulo { Nombre = "Tomate Perita", Descripcion = "Tomates ideales para ensalada o salsa", Precio = 800, Tipo = TipoArticulo.Verdura, ImagenUrl = "https://images.unsplash.com/photo-1592924357228-91a4daadcfea?w=500&auto=format" },
            new Articulo { Nombre = "Lechuga Capuchina", Descripcion = "Lechuga fresca y crujiente", Precio = 600, Tipo = TipoArticulo.Verdura, ImagenUrl = "https://images.unsplash.com/photo-1622206141540-5a45e402b1ed?w=500&auto=format" },
            new Articulo { Nombre = "Naranja", Descripcion = "Naranjas jugosas para jugo", Precio = 1000, Tipo = TipoArticulo.Fruta, ImagenUrl = "https://images.unsplash.com/photo-1547514701-42782101795e?w=500&auto=format" },
            new Articulo { Nombre = "Papa Blanca", Descripcion = "Papas de excelente calidad", Precio = 500, Tipo = TipoArticulo.Verdura, ImagenUrl = "https://images.unsplash.com/photo-1518977676601-b53f02ac6d31?w=500&auto=format" }
        };

        context.Articulos.AddRange(articulos);
        await context.SaveChangesAsync();
    }
}
