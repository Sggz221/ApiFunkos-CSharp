namespace cSharpApiFunko.GraphQl.Inputs;

public record FunkoUpdateInput(
    long Id,
    string? Nombre,
    double? Precio,
    string? Categoria,
    string? Image
);