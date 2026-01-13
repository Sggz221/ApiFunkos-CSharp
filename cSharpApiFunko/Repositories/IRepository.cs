namespace cSharpApiFunko.Repositories;

public interface IRepository<ID, T>
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
}