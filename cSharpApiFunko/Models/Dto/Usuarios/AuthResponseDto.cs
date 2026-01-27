namespace cSharpApiFunko.Models.Dto.Usuarios;

public record AuthResponseDto(
    string Token,
    UserResponseDto User);