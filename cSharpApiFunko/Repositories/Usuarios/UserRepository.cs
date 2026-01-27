using cSharpApiFunko.DataBase;
using cSharpApiFunko.Models;
using Microsoft.EntityFrameworkCore;

namespace cSharpApiFunko.Repositories.Usuarios;

/// <summary>
/// Implementación del repositorio de usuarios
/// </summary>
public class UserRepository(Context context, ILogger<UserRepository> logger) : IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    public async Task<Usuario?> GetByIdAsync(long id)
    {
        logger.LogDebug("Obteniendo usuario por ID: {Id}", id);
        return await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Busca un usuario por su nombre de usuario
    /// </summary>
    public async Task<Usuario?> FindByUsernameAsync(string username)
    {
        logger.LogDebug("Buscando usuario por username: {Username}", username);
        
        if (string.IsNullOrWhiteSpace(username))
        {
            logger.LogWarning("Intento de búsqueda con username vacío o nulo");
            return null;
        }
        
        return await context.Usuarios
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    /// <summary>
    /// Busca un usuario por su email
    /// </summary>
    public async Task<Usuario?> FindByEmailAsync(string email)
    {
        logger.LogDebug("Buscando usuario por email: {Email}", email);
        
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Intento de búsqueda con email vacío o nulo");
            return null;
        }
        
        return await context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Guarda un nuevo usuario en la base de datos
    /// </summary>
    public async Task<Usuario> SaveAsync(Usuario user)
    {
        logger.LogDebug("Guardando nuevo usuario: {Username}", user.UserName);
        
        // Establecer timestamps
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        var saved = await context.Usuarios.AddAsync(user);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Usuario guardado correctamente con ID: {Id}, Username: {Username}", 
            saved.Entity.Id, saved.Entity.UserName);
        
        return saved.Entity;
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    public async Task<Usuario?> UpdateAsync(long id, Usuario user)
    {
        logger.LogDebug("Actualizando usuario con ID: {Id}", id);
        
        var found = await GetByIdAsync(id);
        if (found == null)
        {
            logger.LogWarning("No se pudo actualizar: Usuario con ID {Id} no encontrado", id);
            return null;
        }

        // Actualizar campos
        found.UserName = user.UserName;
        found.Email = user.Email;
        found.PasswordHash = user.PasswordHash;
        found.Role = user.Role;
        found.UpdatedAt = DateTime.UtcNow;

        context.Usuarios.Update(found);
        await context.SaveChangesAsync();

        logger.LogInformation("Usuario actualizado correctamente para ID: {Id}", id);
        return found;
    }

    /// <summary>
    /// Elimina un usuario por su ID
    /// </summary>
    public async Task<Usuario?> DeleteAsync(long id)
    {
        logger.LogDebug("Eliminando usuario con ID: {Id}", id);
        
        var found = await GetByIdAsync(id);
        if (found != null)
        {
            context.Usuarios.Remove(found);
            await context.SaveChangesAsync();
            logger.LogInformation("Usuario con ID {Id} eliminado con éxito", id);
            return found;
        }

        logger.LogWarning("No se pudo eliminar: Usuario con ID {Id} no encontrado", id);
        return null;
    }

    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        logger.LogDebug("Obteniendo todos los usuarios");
        
        var users = await context.Usuarios.ToListAsync();
        
        logger.LogInformation("Se encontraron {Count} usuarios", users.Count);
        return users;
    }
}
