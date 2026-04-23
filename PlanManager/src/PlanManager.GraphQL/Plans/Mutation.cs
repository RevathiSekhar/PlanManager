using MediatR;
using PlanManager.Application.Plans.Commands;

namespace PlanManager.GraphQL.Plans;

public class Mutation
{
    public async Task<PlanResult> CreatePlan(
        CreatePlanInput input,
        [Service] IMediator mediator,
        CancellationToken ct)
        => await mediator.Send(new CreatePlanCommand(input.Title, input.Description), ct);

    public async Task<StepResult> AddStep(
        AddStepInput input,
        [Service] IMediator mediator,
        CancellationToken ct)
        => await mediator.Send(new AddStepCommand(input.PlanId, input.Title, input.Description, input.Order), ct);
}

public record CreatePlanInput(string Title, string? Description);
public record AddStepInput([property: ID] Guid PlanId, string Title, string? Description, int Order);
