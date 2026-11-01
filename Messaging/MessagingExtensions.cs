using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduPlatform.SharedKernel.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services, 
        IConfiguration configuration, 
        params Assembly[] assemblies)
    {
         var serviceName = (Assembly.GetEntryAssembly()?.GetName().Name ?? "service")
              .Replace(".API", "", StringComparison.OrdinalIgnoreCase)
              .ToLowerInvariant();
        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new DefaultEndpointNameFormatter($"{serviceName}-", false));
            foreach (var assembly in assemblies)
            {
                x.AddConsumers(assembly);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
