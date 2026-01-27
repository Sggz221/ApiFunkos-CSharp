
using cSharpApiFunko.Errors;
using cSharpApiFunko.Mappers;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto.Categorias;
using cSharpApiFunko.Repositories.Categorias;
using cSharpApiFunko.Services.Categorias;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;

public class CategoryService (ICategoryRepository repository, IMemoryCache cache) : ICategoryService
{
    private const string CacheKeyPrefix = "Category_";
    private readonly ICategoryRepository _repository = repository;
    private readonly IMemoryCache _cache = cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);


    public async Task<Result<CategoryResponseDto, FunkoError>> GetByIdAsync(Guid id)
    {
        var cacheKey = CacheKeyPrefix +id;

        if (_cache.TryGetValue(cacheKey, out Category? cachedCategory))
        {
            if (cachedCategory != null)
            {
                return cachedCategory.ToDto();
            }
        }
        
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
        {
            return Result.Failure<CategoryResponseDto, FunkoError>(new NotFoundError($"No se encontró la categoría con id: {id}."));
        }
        
        _cache.Set(cacheKey, category, _cacheDuration);
        return category.ToDto();    
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        
        return categories
            .Select(it => it.ToDto())
            .ToList();
    }

    public async Task<Result<CategoryResponseDto, FunkoError>> CreateAsync(CategoryRequestDto dto)
    {
        var alreadyExistingCategory = await _repository.GetByNameAsync(dto.Nombre);

        if (alreadyExistingCategory != null)
        {
            return Result.Failure<CategoryResponseDto, FunkoError>(
                new ConflictError($"La categoría: {dto.Nombre} ya existe."));
        }

        var categoryModel = dto.ToModel();
        
        var savedCategory = await _repository.CreateAsync(categoryModel);
        
        return savedCategory.ToDto();
    }

    public async Task<Result<CategoryResponseDto, FunkoError>> UpdateAsync(Guid id, CategoryRequestDto dto)
    {
        var existingWithSameName = await _repository.GetByNameAsync(dto.Nombre);

        if (existingWithSameName != null && existingWithSameName.Id != id)
        {
            return Result.Failure<CategoryResponseDto, FunkoError>(
                new ConflictError($"Ya existe otra categoría con el nombre: {dto.Nombre}."));
        }
    
        var updatedCategory = await _repository.UpdateAsync(id, new Category(dto.Nombre));

        if (updatedCategory == null)
        {
            return Result.Failure<CategoryResponseDto, FunkoError>(
                new NotFoundError($"No se encontró la categoría con id: {id}."));
        }
    
        _cache.Remove(CacheKeyPrefix + id);
        return updatedCategory.ToDto();
    }

    public async Task<Result<CategoryResponseDto, FunkoError>> DeleteAsync(Guid id)
    {
        var deletedCategory = await _repository.DeleteAsync(id);

        if (deletedCategory == null)
        {
            return Result.Failure<CategoryResponseDto, FunkoError>(
                new NotFoundError($"No se encontró la categoría con id: {id}."));
        }
        
        _cache.Remove(CacheKeyPrefix + id);
        return deletedCategory.ToDto();
    }
}
