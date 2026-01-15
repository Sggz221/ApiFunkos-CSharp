using cSharpApiFunko.Errors;
using cSharpApiFunko.Models.Dto;
using CSharpFunctionalExtensions;

namespace cSharpApiFunko.Services;

public interface IFunkoService
{
    public Task<Result<FunkoResponseDto, FunkoError>> GetByIdAsync(long id);
    public Task<Result<List<FunkoResponseDto>, FunkoError>> GetAllAsync();
    public Task<Result<FunkoResponseDto, FunkoError>> SaveAsync(FunkoRequestDto dto);
    public Task<Result<FunkoResponseDto, FunkoError>> UpdateAsync(long id, FunkoRequestDto dto);
    public Task<Result<FunkoResponseDto, FunkoError>> DeleteAsync(long id);
}