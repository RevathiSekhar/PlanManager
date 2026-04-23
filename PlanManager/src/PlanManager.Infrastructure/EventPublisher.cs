using HotChocolate.Subscriptions;
using PlanManager.Application;
using PlanManager.Domain.Events;

namespace PlanManager.Infrastructure;

public class EventPublisher(ITopicEventSender sender) : IEventPublisher
{
    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            if (domainEvent is PlanCreatedEvent created)
                await sender.SendAsync<IDomainEvent>("plan_created", created, ct);
            else if (domainEvent is StepAddedEvent stepAdded)
                await sender.SendAsync<IDomainEvent>($"plan_updated_{stepAdded.PlanId}", stepAdded, ct);
            else if (domainEvent is PlanUpdatedEvent planUpdated)
                await sender.SendAsync<IDomainEvent>($"plan_updated_{planUpdated.PlanId}", planUpdated, ct);
        }
    }
}
