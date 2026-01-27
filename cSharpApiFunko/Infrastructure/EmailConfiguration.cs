using System.Threading.Channels;
using cSharpApiFunko.Email;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Configuración de servicios de email
/// </summary>
public static class EmailConfiguration
{
    /// <summary>
    /// Configura los servicios de email (MailKit para enviar emails reales a Mailtrap)
    /// </summary>
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        // Canal para comunicación entre servicios (cola de emails)
        services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());
        
        // Servicio de background que procesa la cola de emails
        services.AddHostedService<EmailBackgroundService>();
        
        // Servicio de email
        services.TryAddScoped<IEmailService, MailKitEmailService>();
        
        return services;
    }
}
