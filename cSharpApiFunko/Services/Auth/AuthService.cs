using cSharpApiFunko.Errors;
using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto.Usuarios;
using cSharpApiFunko.Repositories.Usuarios;

namespace cSharpApiFunko.Services.Auth;

using CSharpFunctionalExtensions;

public class AuthService(
    IUserRepository userRepository,
    IJwtService jwtService,
    ILogger<AuthService> logger
) : IAuthService
{

    public async Task<Result<AuthResponseDto, AuthError>> SignUpAsync(UserRequestDto dto)
    {
        var sanitizedUsername = dto.UserName.Replace("\n", "").Replace("\r", "");
        logger.LogInformation("SignUp request for username: {Username}", sanitizedUsername);


        var duplicateCheck = await CheckDuplicatesAsync(dto);
        if (duplicateCheck.IsFailure)
        {
            return Result.Failure<AuthResponseDto, AuthError>(duplicateCheck.Error);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 11);

        var user = new Usuario
        {
            UserName = dto.UserName,
            PasswordHash = passwordHash,
            Email = dto.Email,
            Role = UserRoles.USER
        };

        var savedUser = await userRepository.SaveAsync(user);
        var authResponse = GenerateAuthResponse(savedUser);

        logger.LogInformation("User registered successfully: {Username}", sanitizedUsername);

        return Result.Success<AuthResponseDto, AuthError>(authResponse);
    }
    
    public async Task<Result<AuthResponseDto, AuthError>> SignInAsync(LogInDto dto)
    {
        var sanitizedUsername = dto.Username?.Replace("\n", "").Replace("\r", "");
        logger.LogInformation("SignIn request for username: {Username}", sanitizedUsername);


        var user = await userRepository.FindByUsernameAsync(dto.Username!);
        if (user is null)
        {
            logger.LogWarning("SignIn fallido: Usuario no encontrado - {Username}", sanitizedUsername);
            return Result.Failure<AuthResponseDto, AuthError>(
                new UnauthorizedError("Credenciales inválidas")
            );
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!passwordValid)
        {
            logger.LogWarning("SignIn fallido: Password inválido - {Username}", sanitizedUsername);
            return Result.Failure<AuthResponseDto, AuthError>(
                new UnauthorizedError("Credenciales inválidas")
            );
        }

        var authResponse = GenerateAuthResponse(user);
        logger.LogInformation("Usuario inició sesión correctamente: {Username}", sanitizedUsername);

        return Result.Success<AuthResponseDto, AuthError>(authResponse);
    }

    private async Task<UnitResult<AuthError>> CheckDuplicatesAsync(UserRequestDto dto)
    {
        var existingUser = await userRepository.FindByUsernameAsync(dto.UserName!);
        if (existingUser is not null)
        {
            return UnitResult.Failure<AuthError>(new AuthConflictError("username ya en uso:" + existingUser.UserName));
        }

        var existingEmail = await userRepository.FindByEmailAsync(dto.Email!);
        if (existingEmail is not null)
        {
            return UnitResult.Failure<AuthError>(new AuthConflictError("email ya en uso" + existingEmail.Email));
        }

        return UnitResult.Success<AuthError>();
    }


    private AuthResponseDto GenerateAuthResponse(Usuario user)
    {
        var token = jwtService.GenerateToken(user);

        var userDto = new UserResponseDto(
            user.Id,
            user.UserName,
            user.Email,
            user.Role
            );

        return new AuthResponseDto(token, userDto);
    }
}