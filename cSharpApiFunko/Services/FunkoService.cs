using cSharpApiFunko.Errors;
using cSharpApiFunko.Mappers;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Repositories.Categorias;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace cSharpApiFunko.Services;

public class FunkoService(IFunkoRepository funkoRepository,ICategoryRepository categoryRepository , IMemoryCache cache) : IFunkoService
{
    private static Logger log = LogManager.GetCurrentClassLogger();

    private const string CachePrefix = "funko:";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<Result<FunkoResponseDto, FunkoError>> GetByIdAsync(long id)
    {
        log.Debug($"Buscando funko con ID: {id}");
        var cacheKey = $"{CachePrefix}:{id}";
        if (cache.TryGetValue(cacheKey, out Funko? cachedFunko))
            if (cachedFunko != null)
                return Result.Success<FunkoResponseDto, FunkoError>(cachedFunko.ToResponse());
        
        var funko = await funkoRepository.GetByIdAsync(id);
        return funko != null ? Result.Success<FunkoResponseDto, FunkoError>(funko.ToResponse())
                .Tap(_ => cache.Set(CachePrefix, funko)) : 
            Result.Failure<FunkoResponseDto, FunkoError>(new NotFound($"No se encontro funko con ID: {id}"));
    }

    public async Task<List<FunkoResponseDto>> GetAllAsync()
    {
        log.Debug($"Buscando funko con ID: {CachePrefix}");
        var list = await funkoRepository.GetAllAsync();
        return list.Select(f => f.ToResponse()).ToList();
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> SaveAsync(FunkoRequestDto dto)
    {
        log.Debug($"Guardando funko: {dto}");
        if (!IsValid(dto)) return Result.Failure<FunkoResponseDto, FunkoError>(new Validation("La categoria no es valida"));
        var c = await categoryRepository.GetByIdAsync(dto.Categoria);
        var funko = dto.ToModel();
        funko.Categoria = c!; // No sera null porque lo comprueba IsValid()
        funko.CategoriaId = c!.Id;
        var result = await funkoRepository.SaveAsync(funko);
        return Result.Success<FunkoResponseDto, FunkoError>(result.ToResponse());
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> UpdateAsync(long id, FunkoRequestDto dto)
    {
        log.Debug($"Actualizando funko con ID: {id}");
        var found = await funkoRepository.GetByIdAsync(id);
        if (found == null)
            return Result.Failure<FunkoResponseDto, FunkoError>(new NotFound($"No se encontro funko con id: {id}"));
        if (!IsValid(dto))
            return Result.Failure<FunkoResponseDto, FunkoError>(new Validation("La categoria no es valida"));

        var toSave = dto.ToModel();
        var c =  await categoryRepository.GetByIdAsync(dto.Categoria);
        toSave.Categoria = c!;
        toSave.CategoriaId = c!.Id;
        toSave.UpdatedAt = DateTime.Now;
        await funkoRepository.UpdateAsync(id, toSave);
        cache.Remove(CachePrefix + toSave.Id); // Eliminamos de la cache
        return Result.Success<FunkoResponseDto, FunkoError>(toSave.ToResponse());
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> DeleteAsync(long id)
    {
        log.Debug($"Eliminando funko con ID: {id}");
        var deleted = await funkoRepository.DeleteAsync(id);
        return deleted != null ? Result.Success<FunkoResponseDto, FunkoError>(deleted.ToResponse())
            .Tap(_ => cache.Remove(CachePrefix+id)) :
            Result.Failure<FunkoResponseDto, FunkoError>(new  NotFound($"No se encontro funko con ID: {id}"));
    }

    private bool IsValid(FunkoRequestDto f)
    {
        log.Debug($"Validando funko: {f}");
        return categoryRepository.GetByIdAsync(f.Categoria).Result != null; // null => false | existe => true  
    }
}