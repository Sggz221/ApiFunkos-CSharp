namespace cSharpApiFunko.Models.Dto;

public record FunkoResponseDto(
    long Id,
    string Nombre,
    string Categoria,
    double Precio)
{
    public long Id { get; set; } = Id;
    public string Nombre { get; set; } = Nombre;
    public string Categoria { get; set; } = Categoria;
    public double Precio { get; set; } = Precio;
}