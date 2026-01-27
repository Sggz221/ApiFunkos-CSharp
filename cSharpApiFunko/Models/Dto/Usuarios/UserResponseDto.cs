namespace cSharpApiFunko.Models.Dto.Usuarios;

public record UserResponseDto(
    long Id,
    string UserName,
    string Email,
    string Role
    );