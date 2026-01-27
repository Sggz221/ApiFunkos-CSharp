using System.ComponentModel.DataAnnotations;

namespace cSharpApiFunko.Models.Dto.Usuarios;

public record LogInDto(
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    string Username,

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    string Password
    );