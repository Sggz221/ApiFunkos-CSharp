using cSharpApiFunko.Models;
using Microsoft.EntityFrameworkCore;

namespace cSharpApiFunko.DataBase;

public class Context(DbContextOptions options, ILogger<Context> log) : DbContext(options)
{
    public DbSet<Funko> Funkos { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        SeedData(modelBuilder); // Llamamos al metodo para poblar la BD
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        log.LogInformation("Poblando BD de Categorias...");
        var c1 = new Category("DISNEY");
        var c2 = new Category("MARVEL");
        var c3 = new Category("HARRY_POTTER");
        var c4 = new Category("ANIME");
        var c5 = new Category("HORROR");
        
        modelBuilder.Entity<Category>().HasData(c1, c2, c3, c4, c5);
        log.LogInformation($"Categorias: {c1}, {c2}, {c3}, {c4}, {c5}");
        log.LogInformation("Poblando BD de Funkos...");
        modelBuilder.Entity<Funko>().HasData(
            new Funko { 
                Id = 1, 
                Nombre = "Mickey Mouse", 
                Precio = 12.99, 
                CategoriaId = c1.Id // Disney
            },
            new Funko { 
                Id = 2, 
                Nombre = "Spider-Man", 
                Precio = 15.50, 
                CategoriaId = c2.Id // Marvel
            },
            new Funko { 
                Id = 3, 
                Nombre = "Harry Potter con Búho", 
                Precio = 14.00, 
                CategoriaId = c3.Id // Harry Potter
            },
            new Funko { 
                Id = 4, 
                Nombre = "Goku Super Saiyan", 
                Precio = 18.00, 
                CategoriaId = c4.Id // Anime
            },
            new Funko { 
                Id = 5, 
                Nombre = "Pennywise", 
                Precio = 13.25, 
                CategoriaId = c5.Id // Horror
            },
            new Funko { 
                Id = 6, 
                Nombre = "Iron Man", 
                Precio = 16.00, 
                CategoriaId = c2.Id // Marvel
            },
            new Funko { 
                Id = 7, 
                Nombre = "Naruto Uzumaki (Six Paths)", 
                Precio = 19.99, 
                CategoriaId = c4.Id // ANIME
            },
            new Funko { 
                Id = 8, 
                Nombre = "Stitch (Lilo & Stitch)", 
                Precio = 14.50, 
                CategoriaId = c1.Id // DISNEY
            },
            new Funko { 
                Id = 9, 
                Nombre = "Hermione Granger", 
                Precio = 15.20, 
                CategoriaId = c3.Id // HARRY_POTTER
            },
            new Funko { 
                Id = 10, 
                Nombre = "Capitan America (Endgame)", 
                Precio = 17.00, 
                CategoriaId = c2.Id // MARVEL
            },
            new Funko { 
                Id = 11, 
                Nombre = "Ghostface (Scream)", 
                Precio = 22.50, 
                CategoriaId = c5.Id // HORROR
            },
            new Funko { 
                    Id = 12, 
                    Nombre = "Tanjiro Kamado (Demon Slayer)", 
                    Precio = 18.90, 
                    CategoriaId = c4.Id // ANIME
            }
        );
        log.LogInformation("Funkos cargados en la BD");

        log.LogInformation("Poblando BD de Usuarios...");
        var admin = new Usuario
        {
            Id = 1,
            UserName = "admin",
            Email = "admin@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin", workFactor: 11),
            Role = UserRoles.ADMIN,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var user = new Usuario
        {
            Id = 2,
            UserName = "user",
            Email = "user@user.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user", workFactor: 11),
            Role = UserRoles.USER,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        modelBuilder.Entity<Usuario>().HasData(admin, user);
        log.LogInformation("Usuarios cargados en la BD");
    }
}