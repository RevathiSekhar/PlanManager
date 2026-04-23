using MediatR;
using PlanManager.Application.Plans.Commands;
using PlanManager.Application.Plans.Queries;

namespace PlanManager.GraphQL.Plans;

public class Query
{
    public async Task<PlanResult?> GetPlan(
        [ID] Guid id,
        [Service] IMediator mediator,
        CancellationToken ct)
        => await mediator.Send(new GetPlanQuery(id), ct);

    public async Task<IReadOnlyList<PlanResult>> GetPlans(
        [Service] IMediator mediator,
        CancellationToken ct)
        => await mediator.Send(new GetAllPlansQuery(), ct);
}
