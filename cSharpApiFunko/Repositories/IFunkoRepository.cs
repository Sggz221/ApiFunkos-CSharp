using cSharpApiFunko.Models;

namespace cSharpApiFunko.Repositories;

public interface IFunkoRepository: IRepository<long, Funko>
{
    Task<Funko> SaveAsync(Funko item);
    Task<Funko?> UpdateAsync(long id,  Funko item);
    Task<Funko?> DeleteAsync(Funko item);
}