using PlanManager.Domain.ValueObjects;

namespace PlanManager.Domain.Aggregates;

public class Step
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public PlanTitle Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public int Order { get; private set; }
    public StepStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Step() { } // EF Core

    public static Step Create(Guid planId, string title, string? description, int order)
    {
        return new Step
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Title = PlanTitle.Create(title),
            Description = description,
            Order = order,
            Status = StepStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        Status = StepStatus.Completed;
    }
}

public enum StepStatus { Pending, InProgress, Completed }
