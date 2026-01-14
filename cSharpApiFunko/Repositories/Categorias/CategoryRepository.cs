using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace cSharpApiFunko.Repositories.Categorias;

public class CategoryRepository(Context context): ICategoryRepository
{
    private static Logger log = LogManager.GetCurrentClassLogger();
    
    public async Task<Category?> GetByIdAsync(string nombre)
    {
        log.Debug($"Obteniendo categoria por ID: {nombre}");
        return await context.Categories
            .FirstOrDefaultAsync(c => c.Nombre == nombre);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        log.Debug("Buscando todas las categorias...");
        return await  context.Categories.OrderBy(c => c.Nombre).ToListAsync();
    }
}