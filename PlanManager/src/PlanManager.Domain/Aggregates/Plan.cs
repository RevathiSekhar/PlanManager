using PlanManager.Domain.Events;
using PlanManager.Domain.ValueObjects;

namespace PlanManager.Domain.Aggregates;

public class Plan
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public PlanTitle Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public PlanStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core owns this collection directly — no AsReadOnly() wrapper
    public List<Step> Steps { get; private set; } = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Plan() { }

    public static Plan Create(string title, string? description = null)
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Title = PlanTitle.Create(title),
            Description = description,
            Status = PlanStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        plan._domainEvents.Add(new PlanCreatedEvent(plan.Id, plan.Title.Value));
        return plan;
    }

    public Step AddStep(string title, string? description, int order)
    {
        if (Status == PlanStatus.Completed)
            throw new InvalidOperationException("Cannot add steps to a completed plan.");

        var step = Step.Create(Id, title, description, order);
        Steps.Add(step);
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new StepAddedEvent(Id, step.Id, step.Title.Value));
        return step;
    }

    public void Complete()
    {
        Status = PlanStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new PlanUpdatedEvent(Id, Title.Value, Status));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public enum PlanStatus { Draft, Active, Completed }
