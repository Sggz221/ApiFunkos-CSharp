using cSharpApiFunko.Errors;
using cSharpApiFunko.Models.Dto.Usuarios;
using CSharpFunctionalExtensions;

namespace cSharpApiFunko.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponseDto, AuthError>> SignUpAsync(UserRequestDto dto);
    
    Task<Result<AuthResponseDto, AuthError>> SignInAsync(LogInDto dto);
}