using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace cSharpApiFunko.Models.Dto;

public record FunkoRequestDto(
    string Nombre,
    string Categoria,
    double Precio
    )
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = Nombre;
    [Required(ErrorMessage = "El campo categoria es obligatorio")]
    public string Categoria { get; set; } = Categoria;
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser positivo")]
    public double Precio { get; set; } = Precio;
}