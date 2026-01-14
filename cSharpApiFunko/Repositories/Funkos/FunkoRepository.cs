using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace cSharpApiFunko.Repositories.Categorias;

public class FunkoRepository(Context context): IFunkoRepository
{
    private static Logger log = LogManager.GetCurrentClassLogger();
    
    public async Task<Funko?> GetByIdAsync(long id)
    {
        log.Debug($"Obteniendo funko por id: {id}");
        return await context.Funkos
            .Include(f => f.Categoria)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<Funko>> GetAllAsync()
    {
        log.Debug("Obteniendo todos los funkos...");
        return await context.Funkos
            .Include(f => f.Categoria) // Incluir la navigation property
            .OrderBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<Funko> SaveAsync(Funko item)
    {
        log.Debug($"Guardando funko: {item}");
        var saved = await context.Funkos.AddAsync(item);
        await context.SaveChangesAsync();
        await context.Entry(item).Reference(f => f.Categoria).LoadAsync();
        log.Debug($"Funko guardado correctamente con ID: {item.Id}");
        return saved.Entity;
    }

    public async Task<Funko?> UpdateAsync(long id, Funko item)
    {
        log.Debug($"Actualizando producto con ID: {id} |=> Datos; {item}");
        var found =  await GetByIdAsync(id);
        if (found == null) return null;
        found.Nombre = item.Nombre;
        found.Categoria = item.Categoria;
        found.CategoriaId = item.CategoriaId;
        found.Precio = item.Precio;
        found.UpdatedAt = DateTime.UtcNow;
        var updated = await context.Funkos.AddAsync(found);
        await context.SaveChangesAsync();
        await context.Entry(item).Reference(f => f.Categoria).LoadAsync();
        log.Debug($"Funko actualizado correctamente para ID: {id}");
        return updated.Entity;
    }

    public async Task<Funko?> DeleteAsync(long id)
    {
        log.Debug($"Borrando Funko con ID: {id}");
        var found = await GetByIdAsync(id);
        if (found != null)
        {
            context.Funkos.Remove(found);
            await context.SaveChangesAsync();
            return found;
        }
        log.Error($"No se encontro Funko con ID: {id}");
        return null;
    }
}