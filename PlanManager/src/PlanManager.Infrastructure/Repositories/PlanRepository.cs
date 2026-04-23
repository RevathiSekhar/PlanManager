using Microsoft.EntityFrameworkCore;
using PlanManager.Domain;
using PlanManager.Domain.Aggregates;
using PlanManager.Infrastructure.Persistence;

namespace PlanManager.Infrastructure.Repositories;

public class PlanRepository(PlanDbContext db) : IPlanRepository
{
    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Plans.Include(p => p.Steps).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken ct = default)
        => await db.Plans.Include(p => p.Steps).OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(Plan plan, CancellationToken ct = default)
        => await db.Plans.AddAsync(plan, ct);

    public async Task AddStepAsync(Step step, CancellationToken ct = default)
        => await db.Steps.AddAsync(step, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}
