using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using cSharpApiFunko.Models;

namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Configuración de seguridad (Autenticación y Autorización)
/// </summary>
public static class SecurityConfiguration
{
    /// <summary>
    /// Agrega servicios de seguridad (JWT, Políticas)
    /// </summary>
    public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key missing"));
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });
        
        services.AddAuthorization(options =>
        {
             options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(UserRoles.ADMIN));
             options.AddPolicy("RequireUserRole", policy => policy.RequireRole(UserRoles.USER, UserRoles.ADMIN));
        });
        
        return services;
    }
}
