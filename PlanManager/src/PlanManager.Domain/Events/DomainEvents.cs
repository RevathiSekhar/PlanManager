namespace PlanManager.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record PlanCreatedEvent(Guid PlanId, string Title) : DomainEvent;

public record StepAddedEvent(Guid PlanId, Guid StepId, string StepTitle) : DomainEvent;

public record PlanUpdatedEvent(Guid PlanId, string Title, object Status) : DomainEvent;
