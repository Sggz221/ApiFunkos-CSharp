using cSharpApiFunko.Errors;
using cSharpApiFunko.Models.Dto.Categorias;
using cSharpApiFunko.Services.Categorias;
using Microsoft.AspNetCore.Authorization;

namespace cSharpApiFunko.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
//Hacemos que la url genérica sea /categories, elimina la palabra controller
//y se queda con el nombre de la clase
[Route("[controller]")]
//Especificamos que va a devolver .json
[Produces("application/json")]
public class CategoriesController(ICategoryService service) : ControllerBase
{
    //El path es /categories
    [HttpGet]
    //IEnumerable es la interfaz base de la mayoría de colecciones en 
    //C#, de la que hereda también List
    //Devuelve un código 200 cuyo body es la lista de CategoryDTO
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
    //IActionResult es como el ResponseEntity de Java
    [AllowAnonymous]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await service.GetAllAsync());
    }
    
    //El path es /categories/id
    [HttpGet("{id}", Name = "GetCategoryById")]
    //Devuelve un código 200 con el CategoryDTO como body
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    //Devuelve un código 404 en caso de no encontrarse la Categoría
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await service.GetByIdAsync(id);

        if (result.IsFailure)
        {
            //ojo! el NotFound no es el error de dominio, sino el código 404 de C#
            if (result.Error is NotFoundError)
            {
                return NotFound(new { message = result.Error.Mensaje });
            }
            return BadRequest(new { message = result.Error.Mensaje });
        }

        return Ok(result.Value);
    }
    
    //El path es /categories
    [HttpPost]
    //Devuelve un código 201 con el CategoryDTO como body
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    //Devuelve un código 400 en caso de que el body tenga errores de validación
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //Devuelve un código 409 en caso de que la categoría ya exista
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> PostAsync([FromBody] CategoryRequestDto request)
    {
        var result = await service.CreateAsync(request);

        if (result.IsFailure)
        {
            if (result.Error is ConflictError)
            {
                return Conflict(new { message = result.Error.Mensaje });
            }
            return BadRequest(new { message = result.Error.Mensaje });
        }

        //Devolvemos un 201
        // 1. Nombre de la ruta nombrada (GetCategoryById)
        // 2. Parámetros para esa ruta (el ID de la nueva Categoría)
        // 3. El objeto creado en sí
        return CreatedAtRoute(
            "GetCategoryById",
            new { id = result.Value.Id },
            result.Value);
    }
    
    //El path es /categories/id
    [HttpPut("{id}")]
    //Devuelve un código 200 con el CategoryDTO actualizado como body
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    //Devuelve un código 404 en caso de que la Categoría a actualizar ya exista
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //Devuelve un código 400 en caso de que el body tenga errores de validación
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CategoryRequestDto request)
    {
        var result = await service.UpdateAsync(id, request);

        if (result.IsFailure)
        {
            if (result.Error is NotFoundError)
            {
                return NotFound(new { message = result.Error.Mensaje });
            }
            if (result.Error is ConflictError)
            {
                return Conflict(new { message = result.Error.Mensaje });
            }
            return BadRequest(new { message = result.Error.Mensaje });
        }

        return Ok(result.Value);
    }
    
    //El path es /categories/id
    [HttpDelete("{id}")]
    //Devuelve un código 200 con el CategoryDTO que ha sido eliminada
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    //Devuelve un código 404 en caso de que la Categoría a eliminar no exista
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var result = await service.DeleteAsync(id);

        if (!result.IsFailure) return Ok(result.Value);
        if (result.Error is NotFoundError)
        {
            return NotFound(new { message = result.Error.Mensaje });
        }
        return BadRequest(new { message = result.Error.Mensaje });
    }
}