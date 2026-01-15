using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using Microsoft.EntityFrameworkCore;

namespace cSharpApiFunko.Repositories.Categorias;

public class CategoryRepository(Context context, ILogger<CategoryRepository> log) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(string nombre)
    {
        // Uso de LogDebug con logging estructurado para mayor eficiencia
        log.LogDebug("Obteniendo categoria por nombre: {Nombre}", nombre);
        
        return await context.Categories
            .FirstOrDefaultAsync(c => c.Nombre == nombre);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        log.LogDebug("Buscando todas las categorias...");
        
        return await context.Categories
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }
}