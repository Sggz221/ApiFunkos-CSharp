using cSharpApiFunko.Errors;
using cSharpApiFunko.Mappers;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Repositories.Categorias;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;

namespace cSharpApiFunko.Services.Funkos;

public class FunkoService(
    IFunkoRepository funkoRepository,
    ICategoryRepository categoryRepository, 
    IMemoryCache cache,
    ILogger<FunkoService> log
) : IFunkoService
{
    private const string CachePrefix = "funko:";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<Result<FunkoResponseDto, FunkoError>> GetByIdAsync(long id)
    {
        log.LogDebug("Buscando funko con ID: {Id}", id); // Uso de LogDebug
        
        var cacheKey = $"{CachePrefix}:{id}";
        if (cache.TryGetValue(cacheKey, out Funko? cachedFunko))
        {
            if (cachedFunko != null)
            {
                log.LogInformation("Funko recuperado de la caché: {Id}", id);
                return Result.Success<FunkoResponseDto, FunkoError>(cachedFunko.ToResponse());
            }
        }
        
        var funko = await funkoRepository.GetByIdAsync(id);
        
        if (funko != null)
        {
            return Result.Success<FunkoResponseDto, FunkoError>(funko.ToResponse())
                .Tap(_ => cache.Set(cacheKey, funko, _cacheDuration));
        }

        log.LogWarning("No se encontró el funko con ID: {Id}", id);
        return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con ID: {id}"));
    }

    public async Task<Result<List<FunkoResponseDto>, FunkoError>> GetAllAsync()
    {
        log.LogDebug("Buscando todos los funkos");
        var list = await funkoRepository.GetAllAsync();
        return Result.Success<List<FunkoResponseDto>, FunkoError>
            (list.Select(f => f.ToResponse()).ToList());
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> SaveAsync(FunkoRequestDto dto)
    {
        log.LogInformation("Guardando nuevo funko: {Nombre}", dto.Nombre);
        
        if (!await IsValid(dto)) 
            return Result.Failure<FunkoResponseDto, FunkoError>(new ValidationError("La categoria no es valida"));
        
        var c = await categoryRepository.GetByIdAsync(dto.Categoria);
        var funko = dto.ToModel();
        funko.Categoria = c!;
        funko.CategoriaId = c!.Id;
        
        var result = await funkoRepository.SaveAsync(funko);
        return Result.Success<FunkoResponseDto, FunkoError>(result.ToResponse());
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> UpdateAsync(long id, FunkoRequestDto dto)
    {
        log.LogInformation("Actualizando funko con ID: {Id}", id);
        
        if (!await IsValid(dto))
            return Result.Failure<FunkoResponseDto, FunkoError>(new ValidationError("La categoria no es valida"));

        var toSave = dto.ToModel();
        var c = await categoryRepository.GetByIdAsync(dto.Categoria);
        toSave.Id = id;
        toSave.Categoria = c!;
        toSave.CategoriaId = c!.Id;
        
        var found = await funkoRepository.UpdateAsync(id, toSave);
        if (found == null) 
            return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con id: {id}"));
        
        cache.Remove(CachePrefix + id);
        return Result.Success<FunkoResponseDto, FunkoError>(toSave.ToResponse());
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> DeleteAsync(long id)
    {
        log.LogWarning("Eliminando funko con ID: {Id}", id);
        var deleted = await funkoRepository.DeleteAsync(id);
        
        if (deleted != null)
        {
            cache.Remove(CachePrefix + id);
            return Result.Success<FunkoResponseDto, FunkoError>(deleted.ToResponse());
        }

        return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con ID: {id}"));
    }

    private async Task<bool> IsValid(FunkoRequestDto f)
    {
        log.LogDebug("Validando categoría para funko: {Categoria}", f.Categoria);
        var category = await categoryRepository.GetByIdAsync(f.Categoria);
        return category != null;
    }
}