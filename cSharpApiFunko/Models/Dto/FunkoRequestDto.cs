using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace cSharpApiFunko.Models.Dto;

public record FunkoRequestDto(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    string Nombre,
    [Required(ErrorMessage = "El campo categoria es obligatorio")]
    string Categoria,
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser positivo")]
    double Precio
    )
{
    public string Nombre { get; init; } = Nombre;
    public string Categoria { get; init; } = Categoria;
    public double Precio { get; init; } = Precio;
}