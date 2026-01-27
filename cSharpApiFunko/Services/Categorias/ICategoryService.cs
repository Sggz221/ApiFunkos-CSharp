using cSharpApiFunko.Errors;
using cSharpApiFunko.Models.Dto.Categorias;
using CSharpFunctionalExtensions;

namespace cSharpApiFunko.Services.Categorias;

public interface ICategoryService
{
    Task<Result<CategoryResponseDto, FunkoError>> GetByIdAsync(Guid id);
    Task<List<CategoryResponseDto>> GetAllAsync();
    Task<Result<CategoryResponseDto, FunkoError>> CreateAsync(CategoryRequestDto dto);
    Task<Result<CategoryResponseDto, FunkoError>> UpdateAsync(Guid id, CategoryRequestDto dto);
    Task<Result<CategoryResponseDto, FunkoError>> DeleteAsync(Guid id);
}