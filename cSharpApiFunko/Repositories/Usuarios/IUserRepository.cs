using cSharpApiFunko.Models;

namespace cSharpApiFunko.Repositories.Usuarios;

/// <summary>
/// Repositorio de usuarios para operaciones de acceso a datos
/// </summary>
public interface IUserRepository : IRepository<long, Usuario>
{
    /// <summary>
    /// Busca un usuario por su nombre de usuario
    /// </summary>
    /// <param name="username">Nombre de usuario a buscar</param>
    /// <returns>Usuario encontrado o null si no existe</returns>
    Task<Usuario?> FindByUsernameAsync(string username);
    
    /// <summary>
    /// Busca un usuario por su email
    /// </summary>
    /// <param name="email">Email a buscar</param>
    /// <returns>Usuario encontrado o null si no existe</returns>
    Task<Usuario?> FindByEmailAsync(string email);
    
    /// <summary>
    /// Guarda un nuevo usuario en la base de datos
    /// </summary>
    /// <param name="user">Usuario a guardar</param>
    /// <returns>Usuario guardado con ID asignado</returns>
    Task<Usuario> SaveAsync(Usuario user);
    
    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="id">ID del usuario a actualizar</param>
    /// <param name="user">Datos actualizados del usuario</param>
    /// <returns>Usuario actualizado o null si no existe</returns>
    Task<Usuario?> UpdateAsync(long id, Usuario user);
    
    /// <summary>
    /// Elimina un usuario por su ID
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <returns>Usuario eliminado o null si no existe</returns>
    Task<Usuario?> DeleteAsync(long id);
    
    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    /// <returns>Lista de todos los usuarios</returns>
    Task<IEnumerable<Usuario>> GetAllAsync();
}
