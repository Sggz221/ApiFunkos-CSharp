using cSharpApiFunko.Models;
using cSharpApiFunko.Models.Dto.Categorias;

namespace cSharpApiFunko.Mappers;

public static class CategoryMapper
{
    public static CategoryResponseDto ToDto(this Category category)
    {
        return new CategoryResponseDto()
        {
            Id = category.Id.ToString(),
            Nombre = category.Nombre
        };
    }

    public static Category ToModel(this CategoryRequestDto dto)
    {
        return new Category(dto.Nombre);
    }
}