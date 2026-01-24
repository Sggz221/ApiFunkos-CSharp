using cSharpApiFunko.Errors;
using CSharpFunctionalExtensions;

namespace cSharpApiFunko.Storage;

public interface IStorageService
{
    Task<Result<string, FunkoError>> SaveFileAsync(IFormFile file, string folder);
    Task<Result<bool, FunkoError>> DeleteFileAsync(string fileName);
    bool  FileExists(string fileName);
    string GetFullPath(string fileName);
    string GetRelativePath(string fileName, string folder);
}