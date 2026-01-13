using cSharpApiFunko.Models;

namespace cSharpApiFunko.Repositories;

public class CategoryRepository : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Category>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}