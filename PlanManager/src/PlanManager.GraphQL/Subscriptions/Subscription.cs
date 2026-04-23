using PlanManager.Domain.Events;

namespace PlanManager.GraphQL.Subscriptions;

public class Subscription
{
    [Subscribe]
    [Topic("plan_created")]
    public PlanCreatedPayload OnPlanCreated(
        [EventMessage] IDomainEvent e)
        => e is PlanCreatedEvent created
            ? new PlanCreatedPayload(created.PlanId, created.Title, created.OccurredAt)
            : new PlanCreatedPayload(Guid.Empty, "unknown", DateTime.UtcNow);

    [Subscribe]
    [Topic("plan_updated_{planId}")]
    public PlanUpdatedPayload OnPlanUpdated(
        [ID] Guid planId,
        [EventMessage] IDomainEvent e)
        => e switch
        {
            StepAddedEvent s => new PlanUpdatedPayload(s.PlanId, "StepAdded", $"Step '{s.StepTitle}' was added.", s.OccurredAt),
            PlanUpdatedEvent u => new PlanUpdatedPayload(u.PlanId, "PlanUpdated", $"Plan '{u.Title}' was updated.", u.OccurredAt),
            _ => new PlanUpdatedPayload(planId, "Unknown", "Plan changed.", DateTime.UtcNow)
        };
}

public record PlanCreatedPayload(Guid PlanId, string Title, DateTime OccurredAt);
public record PlanUpdatedPayload(Guid PlanId, string EventType, string Message, DateTime OccurredAt);
