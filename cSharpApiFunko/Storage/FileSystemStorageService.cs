using cSharpApiFunko.Errors;
using CSharpFunctionalExtensions;
using Path = System.IO.Path;

namespace cSharpApiFunko.Storage;

public class FileSystemStorageService: IStorageService
{
    private string _uploadPath;
    private string _rootPath;
    private long _maxFileSize;
    private string[] _allowedExtensions;
    private string[] _allowedContentTypes;
    private ILogger<FileSystemStorageService> _log;

    public FileSystemStorageService(IConfiguration configuration, ILogger<FileSystemStorageService> log, IWebHostEnvironment env)
    {
        _log = log;
        // Configuración desde appsettings.json (ruta relativa a wwwroot)
        _uploadPath = configuration["Storage:UploadPath"] ?? "uploads";
        _maxFileSize = configuration.GetValue<long>("Storage:MaxFileSize", 5 * 1024 * 1024);
        _allowedExtensions = configuration.GetSection("Storage:AllowedExtensions").Get<string[]>()
                             ?? [".jpg", ".jpeg", ".png", ".gif"];
        _allowedContentTypes = configuration.GetSection("Storage:AllowedContentTypes").Get<string[]>()
                               ?? ["image/jpeg", "image/png", "image/gif"];

        // Ruta absoluta: usar WebHostEnvironment.WebRootPath (apunta a wwwroot)
        _rootPath = Path.Combine(env.ContentRootPath, "wwwroot", _uploadPath);

        // Crear directorio si no existe
        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }

        _log.LogInformation("Storage service inicializado en: {Path}", _rootPath);
    }
    
    private static string GenerateUniqueFilename(string originalFilename)
    {
        var extension = System.IO.Path.GetExtension(originalFilename).ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var sanitizedName = System.IO.Path.GetFileNameWithoutExtension(originalFilename)
            .Replace(" ", "_")
            .Replace("-", "_");
        return $"{timestamp}_{uniqueId}_{sanitizedName}{extension}";
    }

    private UnitResult<FunkoError> ValidateFile(IFormFile file)
    {
        if (file is null or { Length: 0 })
        {
            return UnitResult.Failure<FunkoError>(new StorageError("El archivo está vacío"));
        }

        if (file.Length > _maxFileSize)
        {
            return UnitResult.Failure<FunkoError>(
               new StorageError("El archivo es muy grande"));
        }

        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return UnitResult.Failure<FunkoError>(
                new StorageError("La extensión no está permitida"));
        }

        var contentType = file.ContentType?.ToLowerInvariant();
        if (contentType == null || !_allowedContentTypes.Any(ct => contentType.Contains(ct.Split('/')[1])))
        {
            return UnitResult.Failure<FunkoError>(
                new StorageError("El contenido del archivo no está permitido"));
        }
        return UnitResult.Success<FunkoError>();
    }

    public Task<Result<string, FunkoError>> SaveFileAsync(IFormFile file, string folder)
    {
        var validation = ValidateFile(file);
        if (validation.IsFailure)
        {
            return Task.FromResult(Result.Failure<string, FunkoError>(validation.Error));
        }
        try
        {
            // Generar nombre único
            var filename = GenerateUniqueFilename(file.FileName);

            // Crear directorio destino
            var folderPath = Path.Combine(_rootPath, folder);
            Directory.CreateDirectory(folderPath);

            // Guardar ficheiro
            var filePath = Path.Combine(folderPath, filename);
            var relativePath = GetRelativePath(filename, folder);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            _log.LogInformation("Archivo guardado: {Path}", relativePath);

            return Task.FromResult(Result.Success<string, FunkoError>(relativePath));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error guardando archivo");
            return Task.FromResult(Result.Failure<string, FunkoError>(
                new StorageError("Error guardando archivo")));
        }
    }

    public Task<Result<bool, FunkoError>> DeleteFileAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return Task.FromResult(Result.Success<bool, FunkoError>(true));
        }

        try
        {
            var fullPath = GetFullPath(fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _log.LogInformation("Archivo eliminado: {Filename}", fileName);
            }

            return Task.FromResult(Result.Success<bool, FunkoError>(true));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error eliminando archivo {Filename}", fileName);
            return Task.FromResult(Result.Failure<bool, FunkoError>(
                new StorageError("Error eliminando archivo")));
        }
    }

    public bool FileExists(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var fullPath = GetFullPath(fileName);
        return File.Exists(fullPath);
    }

    public string GetFullPath(string fileName)
    {
        if (Path.IsPathRooted(fileName))
            return fileName;

        var cleanFilename = fileName;
        var prefix = $"/{_uploadPath}/";

        if (fileName.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase))
            cleanFilename = fileName["/storage/".Length..];
        else if (fileName.StartsWith("/storage", StringComparison.OrdinalIgnoreCase))
            cleanFilename = fileName["/storage".Length..].TrimStart('/');
        else if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            cleanFilename = fileName[prefix.Length..];

        return Path.Combine(_rootPath, cleanFilename);
    }

    public string GetRelativePath(string fileName,  string folder)
    {
        return $"/{_uploadPath}/{folder}/{fileName}";
    }
}