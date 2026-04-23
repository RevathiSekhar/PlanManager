using MediatR;
using PlanManager.Domain;
using PlanManager.Domain.Aggregates;
using PlanManager.Domain.Events;

namespace PlanManager.Application.Plans.Commands;

// ── Create Plan ───────────────────────────────────────────────────────────────

public record CreatePlanCommand(string Title, string? Description) : IRequest<PlanResult>;

public class CreatePlanHandler(IPlanRepository repository, IEventPublisher publisher)
    : IRequestHandler<CreatePlanCommand, PlanResult>
{
    public async Task<PlanResult> Handle(CreatePlanCommand request, CancellationToken ct)
    {
        var plan = Plan.Create(request.Title, request.Description);
        await repository.AddAsync(plan, ct);
        await repository.SaveChangesAsync(ct);
        await publisher.PublishAsync(plan.DomainEvents, ct);
        plan.ClearDomainEvents();
        return PlanResult.From(plan);
    }
}

// ── Add Step ──────────────────────────────────────────────────────────────────

public record AddStepCommand(Guid PlanId, string Title, string? Description, int Order) : IRequest<StepResult>;

public class AddStepHandler(IPlanRepository repository, IEventPublisher publisher)
    : IRequestHandler<AddStepCommand, StepResult>
{
    public async Task<StepResult> Handle(AddStepCommand request, CancellationToken ct)
    {
        try
        {
            Console.WriteLine($">>> AddStep called: PlanId={request.PlanId}, Title={request.Title}");

            var plan = await repository.GetByIdAsync(request.PlanId, ct)
                ?? throw new System.Collections.Generic.KeyNotFoundException($"Plan {request.PlanId} not found.");

            Console.WriteLine($">>> Plan found: {plan.Id}, Steps count: {plan.Steps.Count}");

            var step = plan.AddStep(request.Title, request.Description, request.Order);

            Console.WriteLine($">>> Step created: {step.Id}");

            await repository.AddStepAsync(step, ct);

            Console.WriteLine(">>> Step added to context, saving...");

            await repository.SaveChangesAsync(ct);

            Console.WriteLine(">>> Saved successfully!");

            await publisher.PublishAsync(plan.DomainEvents, ct);
            plan.ClearDomainEvents();

            return StepResult.From(step);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("=== ADD STEP EXCEPTION ===");
            Console.WriteLine($"Type   : {ex.GetType().FullName}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack  : {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner  : {ex.InnerException.Message}");
                Console.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
            }
            Console.ResetColor();
            throw;
        }
    }
}

// ── Result DTOs ───────────────────────────────────────────────────────────────

public record PlanResult(Guid Id, string Title, string? Description, string Status, DateTime CreatedAt, DateTime UpdatedAt, IReadOnlyList<StepResult> Steps)
{
    public static PlanResult From(Plan plan) => new(
        plan.Id, plan.Title.Value, plan.Description,
        plan.Status.ToString(), plan.CreatedAt, plan.UpdatedAt,
        plan.Steps.Select(StepResult.From).ToList());
}

public record StepResult(Guid Id, Guid PlanId, string Title, string? Description, int Order, string Status, DateTime CreatedAt)
{
    public static StepResult From(Step step) => new(
        step.Id, step.PlanId, step.Title.Value, step.Description,
        step.Order, step.Status.ToString(), step.CreatedAt);
}
