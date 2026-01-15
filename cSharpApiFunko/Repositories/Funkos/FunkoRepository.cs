using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Indispensable para ILogger

namespace cSharpApiFunko.Repositories.Categorias;

public class FunkoRepository(Context context, ILogger<FunkoRepository> log) : IFunkoRepository
{
    public async Task<Funko?> GetByIdAsync(long id)
    {
        log.LogDebug("Obteniendo funko por id: {Id}", id);
        return await context.Funkos
            .Include(f => f.Categoria)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<Funko>> GetAllAsync()
    {
        log.LogDebug("Obteniendo todos los funkos...");
        return await context.Funkos
            .Include(f => f.Categoria)
            .OrderBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<Funko> SaveAsync(Funko item)
    {
        log.LogDebug("Guardando funko: {Nombre}", item.Nombre);
        var saved = await context.Funkos.AddAsync(item);
        await context.SaveChangesAsync();
        
        // Carga la relación para que el objeto devuelto esté completo
        await context.Entry(item).Reference(f => f.Categoria).LoadAsync();
        
        log.LogInformation("Funko guardado correctamente con ID: {Id}", item.Id);
        return saved.Entity;
    }

    public async Task<Funko?> UpdateAsync(long id, Funko item)
    {
        log.LogDebug("Actualizando producto con ID: {Id}", id);
        
        var found = await GetByIdAsync(id);
        if (found == null) 
        {
            log.LogWarning("No se pudo actualizar: Funko con ID {Id} no encontrado", id);
            return null;
        }

        found.Nombre = item.Nombre;
        found.CategoriaId = item.CategoriaId;
        found.Precio = item.Precio;
        found.UpdatedAt = DateTime.UtcNow;

        context.Funkos.Update(found); // Uso de Update para asegurar el seguimiento
        await context.SaveChangesAsync();
        await context.Entry(found).Reference(f => f.Categoria).LoadAsync();

        log.LogInformation("Funko actualizado correctamente para ID: {Id}", id);
        return found;
    }

    public async Task<Funko?> DeleteAsync(long id)
    {
        log.LogDebug("Borrando Funko con ID: {Id}", id);
        
        var found = await GetByIdAsync(id);
        if (found != null)
        {
            context.Funkos.Remove(found);
            await context.SaveChangesAsync();
            log.LogInformation("Funko con ID {Id} borrado con éxito", id);
            return found;
        }

        log.LogError("Error al borrar: No se encontro Funko con ID: {Id}", id);
        return null;
    }
}