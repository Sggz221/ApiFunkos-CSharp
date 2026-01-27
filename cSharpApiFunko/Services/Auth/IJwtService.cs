using cSharpApiFunko.Models;

namespace cSharpApiFunko.Services.Auth;

public interface IJwtService
{
    string GenerateToken(Usuario user);

    string? ValidateToken(string token);
}