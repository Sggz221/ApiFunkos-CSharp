using cSharpApiFunko.Errors;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Services.Funkos;
using cSharpApiFunko.Storage;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Path = System.IO.Path;

namespace cSharpApiFunko.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class FunkoController(IFunkoService service, ILogger<FunkoController> log, IStorageService storageService): ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FunkoResponseDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? nombre = null,
        [FromQuery] string? categoria  = null,
        [FromQuery] double? maxPrecio = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortby = "id",
        [FromQuery] string direction = "asc")
    {
        log.LogInformation("Obteniendo todos los Funkos paginados");
        var filter = new FilterDto(nombre, categoria, maxPrecio, page, size, direction);
        var result = await service.GetAllAsync(filter);
        return result.Match(
            onSuccess: Ok,
            onFailure:error => error switch
            {
                NotFoundError => NotFound(new {message = error.Mensaje}),
                _ => StatusCode(500, new { message = error.Mensaje})
            }
            );
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(long id)
    {
        log.LogInformation($"Obteniendo productos con ID: {id}");
        var result = await service.GetByIdAsync(id);
        return result.Match(
            onSuccess: Ok,
            onFailure:error => error switch
            {
                Errors.NotFoundError => NotFound(new {message=error.Mensaje}),
                _ => StatusCode(500, new { message = error.Mensaje })
            }
            );
    }

    [HttpPost]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Post([FromForm] FunkoRequestDto request, [FromForm] IFormFile? file)
    {
        log.LogInformation($"Creando nuevo funko: {request.Nombre}");

        if (file != null)
        {
            var relativePath = await storageService.SaveFileAsync(file, "funkos");
            if (relativePath.IsFailure) return BadRequest(new { message = relativePath.Error.Mensaje });
            request.Image = relativePath.Value;
        }

        if (string.IsNullOrEmpty(request.Image))
            return BadRequest(new { message = "El campo 'imagen' es obligatorio." });
        
        var result = await service.SaveAsync(request);
        
        return result.Match(
            onSuccess: f => CreatedAtAction(nameof(GetById), new {id=f.Id}, f),
            onFailure: error => error switch
            {
                ValidationError => BadRequest(new { message = error.Mensaje }),
                Errors.ConflictError => Conflict(new {message = error.Mensaje}),
                _ => StatusCode(500, error.Mensaje)
            }
        );
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Put(long id, [FromForm] FunkoRequestDto request,[FromForm] IFormFile? file)
    {
        log.LogInformation($"Actualizando funko con ID: {id}");
        
        if (file != null)
        {
            var relativePath = await storageService.SaveFileAsync(file, "funkos");
            if (relativePath.IsFailure) return BadRequest(new { message = relativePath.Error.Mensaje });
            request.Image = relativePath.Value;
        }

        if (string.IsNullOrEmpty(request.Image))
            return BadRequest(new { message = "El campo 'imagen' es obligatorio." });
        
        var result = await service.UpdateAsync(id, request);
        return result.Match(
            onSuccess: funko => Ok(funko),
            onFailure: error => error switch
            {
                Errors.NotFoundError => NotFound(new { message = error.Mensaje }),
                ValidationError => BadRequest(new { message = error.Mensaje }),
                _ => StatusCode(500, error.Mensaje)
            }
        );
    }
    
    //El path es /funkos/id
    [HttpPatch("{id}")]
    //Devuelve un código 200 con el FunkoDTO actualizado parcialmente como body
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    //Devuelve un código 404 en caso de que el Funko a actualizar no exista
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //Devuelve un código 409 en caso de que la categoría nueva no sea válida
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> PatchAsync(long id, [FromForm] FunkoPatchRequestDto request,[FromForm] IFormFile? file)
    {
        if (file != null)
        {
            var relativePath = await storageService.SaveFileAsync(file, "funkos");
            if (relativePath.IsFailure) return BadRequest(new { message = relativePath.Error.Mensaje });
            request.Image = relativePath.Value;
        }
        
        var result = await service.PatchAsync(id, request);

        if (!result.IsFailure) return Ok(result.Value);
        
        return result.Error switch
        {
            NotFoundError => NotFound(new { message = result.Error.Mensaje }),
            ConflictError => Conflict(new { message = result.Error.Mensaje }),
            _ => BadRequest(new { message = result.Error.Mensaje })
        };
    }
    
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Delete(long id)
    {
        log.LogInformation($"Borrando funko con ID: {id}");
        var result = await service.DeleteAsync(id);
        return result.Match(
            onSuccess: funko =>
            {
                storageService.DeleteFileAsync(Path.GetFileName(funko.Image));
                return Ok(funko);
            },
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Mensaje }),
                _ => StatusCode(500, error.Mensaje)
            }
        );
    }
}