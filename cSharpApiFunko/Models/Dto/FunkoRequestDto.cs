namespace cSharpApiFunko.Models.Dto;

public record FunkoRequestDto(
    string Nombre,
    string Categoria,
    double Precio
    )
{
    public string Nombre { get; set; } = Nombre;
    public string Categoria { get; set; } = Categoria;
    public double Precio { get; set; } = Precio;
}