using PlanManager.Domain.Events;

namespace PlanManager.Application;

public interface IEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
