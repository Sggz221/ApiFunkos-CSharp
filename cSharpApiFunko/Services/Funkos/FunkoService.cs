using cSharpApiFunko.Email;
using cSharpApiFunko.Errors;
using cSharpApiFunko.Mappers;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Notifications;
using cSharpApiFunko.Repositories.Categorias;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace cSharpApiFunko.Services.Funkos;

public class FunkoService(
    IFunkoRepository funkoRepository,
    ICategoryRepository categoryRepository, 
    IDistributedCache cache,
    ILogger<FunkoService> log,
    IEmailService emailService,
    IConfiguration configuration,
    IHubContext<FunkoHub> hubContext) : IFunkoService
{
    private const string CachePrefix = "funko:";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
    
    public async Task<Result<FunkoResponseDto, FunkoError>> GetByIdAsync(long id)
    {
        log.LogDebug("Buscando funko con ID: {Id}", id);
        
        var cacheKey = $"{CachePrefix}:{id}";
        var cachedData = await cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            var cachedFunko = JsonSerializer.Deserialize<Funko>(cachedData, JsonOptions);
            if (cachedFunko != null)
            {
                log.LogInformation("Funko recuperado de la caché: {Id}", id);
                return Result.Success<FunkoResponseDto, FunkoError>(cachedFunko.ToResponse());
            }
        }
        
        var funko = await funkoRepository.GetByIdAsync(id);
        
        if (funko != null)
        {
            var serialized = JsonSerializer.Serialize(funko, JsonOptions);
            await cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            });
            return Result.Success<FunkoResponseDto, FunkoError>(funko.ToResponse());
        }

        log.LogWarning("No se encontró el funko con ID: {Id}", id);
        return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con ID: {id}"));
    }

    public async Task<Result<PageResponse<FunkoResponseDto>, FunkoError>> GetAllAsync(FilterDto filter)
    {
        log.LogInformation($"Buscando funkos | Pagina: {filter.Page}, Size: {filter.Size}");

        var (funkos, totalCount) = await funkoRepository.GetAllAsync(filter);
        var response = funkos.Select(it => it.ToResponse()).ToList();

        var page = new PageResponse<FunkoResponseDto>
        {
            Items = response,
            TotalCount = totalCount,
            Page = filter.Page,
            Size = filter.Size
        };
        
        return Result.Success<PageResponse<FunkoResponseDto>, FunkoError>(page);
    }


    public async Task<Result<FunkoResponseDto, FunkoError>> SaveAsync(FunkoRequestDto dto)
    {
        log.LogInformation("Guardando nuevo funko: {Nombre}", dto.Nombre);
        
        if (!await IsValid(dto)) 
            return Result.Failure<FunkoResponseDto, FunkoError>(new ValidationError("La categoria no es valida"));
        
        var c = await categoryRepository.GetByNameAsync(dto.Categoria);
        var funko = dto.ToModel();
        funko.Categoria = c!;
        funko.CategoriaId = c!.Id;
        
        var result = await funkoRepository.SaveAsync(funko);
        // Notificacion
        await hubContext.Clients.Group("admins").SendAsync("NuevoFunko", new
        {
            result.Id,
            result.Nombre,
            result.Categoria,
            result.Precio,
            result.CreatedAt,
            result.UpdatedAt
        });
        // Email
        EnviarEmail(result);
        log.LogDebug("Notificacion de creado enviada");
        return Result.Success<FunkoResponseDto, FunkoError>(result.ToResponse());
    }

    public async Task<Result<FunkoResponseDto, FunkoError>> UpdateAsync(long id, FunkoRequestDto dto)
    {
        log.LogInformation("Actualizando funko con ID: {Id}", id);
        
        if (!await IsValid(dto))
            return Result.Failure<FunkoResponseDto, FunkoError>(new ValidationError("La categoria no es valida"));

        var toSave = dto.ToModel();
        var c = await categoryRepository.GetByNameAsync(dto.Categoria);
        toSave.Id = id;
        toSave.Categoria = c!;
        toSave.CategoriaId = c!.Id;
        
        var found = await funkoRepository.UpdateAsync(id, toSave);
        if (found == null) 
            return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con id: {id}"));
        // Notificacion
        await hubContext.Clients.Group("admins").SendAsync("FunkoActualizado", new
        {
            found.Id,
            found.Nombre,
            found.Categoria,
            found.Precio,
            found.CreatedAt,
            found.UpdatedAt
        });
        log.LogDebug("Notificacion de actualizacoin enviada");
        
        await cache.RemoveAsync(CachePrefix + id);
        return Result.Success<FunkoResponseDto, FunkoError>(toSave.ToResponse());
    }
    
    public async Task<Result<FunkoResponseDto, FunkoError>> PatchAsync(long id, FunkoPatchRequestDto dto)
    {
        var foundFunko = await funkoRepository.GetByIdAsync(id);
        
        if (foundFunko == null) return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"Funko {id} no encontrado"));
        
        if (dto.Nombre != null) foundFunko.Nombre = dto.Nombre;
        
        if (dto.Precio != null) foundFunko.Precio = (double)dto.Precio;
        
        if (dto.Image != null) foundFunko.Image =  dto.Image;

        if (dto.Categoria != null)
        {
            var foundCategory = await categoryRepository.GetByNameAsync(dto.Categoria);
            if (foundCategory == null)
            {
                return Result.Failure<FunkoResponseDto, FunkoError>(new ConflictError($"La categoría: {dto.Categoria} no existe."));
            }
            // Asignarmos el CategoryId obtenido de la búsqueda
            // Para establecer la relación de FK correctamente
            foundFunko.Categoria = foundCategory;
            foundFunko.CategoriaId = foundCategory.Id;
        }

        await funkoRepository.UpdateAsync(id, foundFunko);
        
        await hubContext.Clients.Group("admins").SendAsync("FunkoActualizadoPatch", new
        {
            foundFunko.Id,
            foundFunko.Nombre,
            foundFunko.Categoria,
            foundFunko.Precio,
            foundFunko.CreatedAt,
            foundFunko.UpdatedAt
        });
        log.LogDebug("Notificacion de actualizacion enviada (Patch)");
        await cache.RemoveAsync(CachePrefix + id);
        return foundFunko.ToResponse();
    }
    
    public async Task<Result<FunkoResponseDto, FunkoError>> DeleteAsync(long id)
    {
        log.LogWarning("Eliminando funko con ID: {Id}", id);
        var deleted = await funkoRepository.DeleteAsync(id);
        
        if (deleted != null)
        {
            await cache.RemoveAsync(CachePrefix + id);
            await hubContext.Clients.Group("admins").SendAsync("FunkoEliminado", new
            {
                deleted.Id,
                deleted.Nombre,
                deleted.Categoria,
                deleted.Precio,
                deleted.CreatedAt,
                deleted.UpdatedAt
            });
            log.LogDebug("Notificacion de eliminacion enviada");
            return Result.Success<FunkoResponseDto, FunkoError>(deleted.ToResponse());
        }
        return Result.Failure<FunkoResponseDto, FunkoError>(new NotFoundError($"No se encontro funko con ID: {id}"));
    }

    private async Task<bool> IsValid(FunkoRequestDto f)
    {
        log.LogDebug("Validando categoría para funko: {Categoria}", f.Categoria);
        var category = await categoryRepository.GetByNameAsync(f.Categoria);
        if (category == null)
        {
            log.LogError($"La categoria no es valida. categoria encontrada: {category}");
            return false;
        }
        log.LogDebug($"Categoria validada correctamente. Categoria: {category}");
        return true;
    }
    
    private void EnviarEmail(Funko funko)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var adminEmail = configuration["Smtp:AdminEmail"];
                if (string.IsNullOrEmpty(adminEmail)) return;

                var content = EmailTemplates.ProductoCreado(funko.Nombre, funko.Precio, funko.Categoria!.Nombre, funko.Id);
                var body = EmailTemplates.CreateBase("Nuevo Producto Creado", content);

                var emailMessage = new EmailMessage
                {
                    To = adminEmail,
                    Subject = "🆕 Nuevo Producto en Tienda DAW",
                    Body = body,
                    IsHtml = true
                };
                await emailService.EnqueueEmailAsync(emailMessage);
                log.LogDebug("Email de notificación encolado tras crear producto");
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Error al encolar email de notificación tras crear producto");
            }
        });
    }
}