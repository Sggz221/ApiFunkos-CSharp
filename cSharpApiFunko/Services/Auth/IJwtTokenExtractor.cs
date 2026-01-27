namespace cSharpApiFunko.Services.Auth;
using System.Security.Claims;

public interface IJwtTokenExtractor
{

    long? ExtractUserId(string token);

    string? ExtractRole(string token);

    bool IsAdmin(string token);

    (long? UserId, bool IsAdmin, string? Role) ExtractUserInfo(string token);

    ClaimsPrincipal? ExtractClaims(string token);

    string? ExtractEmail(string token);

    bool IsValidTokenFormat(string token);
}