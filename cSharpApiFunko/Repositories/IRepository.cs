namespace cSharpApiFunko.Repositories;

public interface IRepository<ID, T>
{
    Task<T?> GetByIdAsync(ID id);
}