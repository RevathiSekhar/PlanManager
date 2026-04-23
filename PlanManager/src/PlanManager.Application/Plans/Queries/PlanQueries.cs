using MediatR;
using PlanManager.Application.Plans.Commands;
using PlanManager.Domain;

namespace PlanManager.Application.Plans.Queries;

public record GetPlanQuery(Guid PlanId) : IRequest<PlanResult?>;

public class GetPlanHandler(IPlanRepository repository) : IRequestHandler<GetPlanQuery, PlanResult?>
{
    public async Task<PlanResult?> Handle(GetPlanQuery request, CancellationToken ct)
    {
        var plan = await repository.GetByIdAsync(request.PlanId, ct);
        return plan is null ? null : PlanResult.From(plan);
    }
}

public record GetAllPlansQuery : IRequest<IReadOnlyList<PlanResult>>;

public class GetAllPlansHandler(IPlanRepository repository) : IRequestHandler<GetAllPlansQuery, IReadOnlyList<PlanResult>>
{
    public async Task<IReadOnlyList<PlanResult>> Handle(GetAllPlansQuery request, CancellationToken ct)
    {
        var plans = await repository.GetAllAsync(ct);
        return plans.Select(PlanResult.From).ToList();
    }
}
