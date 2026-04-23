using PlanManager.Domain.Aggregates;

namespace PlanManager.Domain;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Plan plan, CancellationToken ct = default);
    Task AddStepAsync(Step step, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
