using cSharpApiFunko.Errors;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cSharpApiFunko.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FunkoController(IFunkoService service, ILogger<FunkoController> log): ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FunkoResponseDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        log.LogInformation("Obteniendo todos los Funkos...");
        var result = await service.GetAllAsync();
        return result.Match(
            onSuccess: funko => Ok(funko),
            onFailure:error => error switch
            {
                Errors.NotFound => NotFound(new {message = error.Mensaje}),
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
            onSuccess: funko => Ok(funko),
            onFailure:error => error switch
            {
                Errors.NotFound => NotFound(new {message=error.Mensaje}),
                _ => StatusCode(500, new { message = error.Mensaje })
            }
            );
    }

    [HttpPost]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Post([FromBody] FunkoRequestDto request)
    {
        log.LogInformation($"Creando nuevo funko: {request.Nombre}");
        var result = await service.SaveAsync(request);
        return result.Match(
            onSuccess: funko => Ok(funko),
            onFailure: error => error switch
            {
                Validation => BadRequest(new { message = error.Mensaje }),
                Errors.Conflict => Conflict(new {message = error.Mensaje}),
                _ => StatusCode(500, error.Mensaje)
            }
        );
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Put(long id, [FromBody] FunkoRequestDto request)
    {
        log.LogInformation($"Actualizando funko con ID: {id}");
        var result = await service.UpdateAsync(id, request);
        return result.Match(
            onSuccess: funko => Ok(funko),
            onFailure: error => error switch
            {
                Errors.NotFound => NotFound(new { message = error.Mensaje }),
                Validation => BadRequest(new { message = error.Mensaje }),
                _ => StatusCode(500, error.Mensaje)
            }
        );
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(long id)
    {
        log.LogInformation($"Borrando funko con ID: {id}");
        var result = await service.DeleteAsync(id);
        return result.Match(
            onSuccess: funko => Ok(funko),
            onFailure: error => error switch
            {
                Errors.NotFound => NotFound(new { message = error.Mensaje }),
                _ => StatusCode(500, error.Mensaje)
            }
        );
    }
}