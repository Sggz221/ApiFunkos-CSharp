using System.ComponentModel.DataAnnotations;

namespace cSharpApiFunko.Models.Dto.Categorias;

public class CategoryRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
}