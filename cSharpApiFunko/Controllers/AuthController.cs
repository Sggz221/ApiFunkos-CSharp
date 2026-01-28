using cSharpApiFunko.Errors;
using cSharpApiFunko.Models.Dto.Usuarios;
using cSharpApiFunko.Services.Auth;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace cSharpApiFunko.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger
) : ControllerBase
{
    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] UserRequestDto dto)
    {
        logger.LogInformation("Signup request received for user: {Username}", dto.UserName);

        var resultado = await authService.SignUpAsync(dto);

        return resultado.Match(
            response => CreatedAtAction(nameof(SignUp), response),
            error => error switch
            {
                AuthConflictError conflictError => Conflict(new { message = conflictError.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }
    
    [HttpPost("signin")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] LogInDto dto)
    {
        logger.LogInformation("Petición de inicio de sesión recibida para usuario: {Username}", dto.Username);

        var resultado = await authService.SignInAsync(dto);

        return resultado.Match(
            response => Ok(response),
            error => error switch
            {
                UnauthorizedError unauthorizedError => Unauthorized(new { message = unauthorizedError.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }
}