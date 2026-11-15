using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduPlatform.SharedKernel.Messaging;

public static class MessagingExtensions
{
    /// <summary>
    /// RabbitMQ messaging WITHOUT outbox: consumers get retry, Publish() sends directly to RabbitMQ.
    /// Kept for services that have not been switched to the outbox yet.
    /// </summary>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        services.AddMassTransit(x =>
        {
            ConfigureBase(x, assemblies);

            x.AddConfigureEndpointsCallback((context, name, cfg) => UseRetry(cfg));

            UseRabbitMq(x, configuration);
        });

        return services;
    }

    /// <summary>
    /// RabbitMQ messaging WITH transactional outbox + inbox (MassTransit Entity Framework outbox).
    /// - Sender: Publish() writes an OutboxMessage into TDbContext; it is stored in the SAME SaveChanges as the
    ///   business data and delivered to RabbitMQ by a background job (also after RabbitMQ outages / restarts).
    ///   => Publish() must happen BEFORE SaveChangesAsync, otherwise the OutboxMessage is never saved.
    /// - Consumer: retry first, then inbox (InboxState) so redelivered messages are processed only once.
    /// Requires modelBuilder.AddTransactionalOutboxEntities() in TDbContext.OnModelCreating + a migration.
    /// </summary>
    public static IServiceCollection AddMessaging<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
        where TDbContext : DbContext
    {
        services.AddMassTransit(x =>
        {
            ConfigureBase(x, assemblies);

            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(1);
            });

            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                UseRetry(cfg);                                   // retry wraps ...
                cfg.UseEntityFrameworkOutbox<TDbContext>(context); // ... the inbox/consumer outbox
            });

            UseRabbitMq(x, configuration);
        });

        return services;
    }

    // queue names get a per-service prefix ("course-...") so two services never share one queue
    private static void ConfigureBase(IBusRegistrationConfigurator x, Assembly[] assemblies)
    {
        var serviceName = (Assembly.GetEntryAssembly()?.GetName().Name ?? "service")
            .Replace(".API", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        x.SetEndpointNameFormatter(new DefaultEndpointNameFormatter($"{serviceName}-", false));

        foreach (var assembly in assemblies)
            x.AddConsumers(assembly);
    }

    // transient consumer errors (DB timeout, ...): 5 attempts with growing delay before the message goes to *_error.
    // No delayed redelivery: the RabbitMQ image has no delayed-message-exchange plugin.
    private static void UseRetry(IReceiveEndpointConfigurator cfg)
    {
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromSeconds(30),
            intervalDelta: TimeSpan.FromSeconds(2)));
    }

    private static void UseRabbitMq(IBusRegistrationConfigurator x, IConfiguration configuration)
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
            {
                h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(configuration["RabbitMQ:Password"] ?? "guest");
            });
            cfg.ConfigureEndpoints(context);
        });
    }
}
