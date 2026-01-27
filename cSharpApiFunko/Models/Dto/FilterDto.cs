namespace cSharpApiFunko.Models.Dto;

public record FilterDto(
    string? Nombre,
    string? Categoria,
    double? MaxPrecio,
    int Page = 1,
    int Size = 10,
    string SortBy = "id",
    string Direction = "asc"
    );