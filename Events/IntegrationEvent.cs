namespace EduPlatform.SharedKernel.Events;

// the base for any event that leaves a microservice to go to another
// having standard ID and timestamp across all of them helps us trace messages in RabbitMQ later
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
