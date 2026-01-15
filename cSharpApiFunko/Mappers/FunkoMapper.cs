using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto;

namespace cSharpApiFunko.Mappers;

public static class FunkoMapper
{
    public static Funko ToModel(this FunkoRequestDto dto)
    {
        return new Funko()
        {
            Id = 0,
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static FunkoResponseDto ToResponse(this Funko model)
    {
        return new FunkoResponseDto(
            model.Id,
            model.Nombre,
            model.Categoria!.Nombre,
            model.Precio);
    }
}