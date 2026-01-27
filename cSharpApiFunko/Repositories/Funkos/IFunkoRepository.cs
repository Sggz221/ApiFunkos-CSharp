using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto;

namespace cSharpApiFunko.Repositories.Categorias;

public interface IFunkoRepository: IRepository<long, Funko>
{
    Task<(IEnumerable<Funko> Items, int TotalCount)> GetAllAsync(FilterDto filter);
    Task<Funko> SaveAsync(Funko item);
    Task<Funko?> UpdateAsync(long id,  Funko item);
    Task<Funko?> DeleteAsync(long id);
}