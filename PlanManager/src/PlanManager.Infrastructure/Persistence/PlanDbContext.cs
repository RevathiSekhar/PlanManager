using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanManager.Domain.Aggregates;
using PlanManager.Domain.ValueObjects;

namespace PlanManager.Infrastructure.Persistence;

public class PlanDbContext(DbContextOptions<PlanDbContext> options) : DbContext(options)
{
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Step> Steps => Set<Step>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new StepConfiguration());
    }
}

internal class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Title)
         .HasConversion(t => t.Value, v => PlanTitle.Create(v))
         .HasMaxLength(200)
         .IsRequired();
        b.Property(p => p.Description).HasMaxLength(2000);
        b.Property(p => p.Status).HasConversion<string>();
        b.Property(p => p.CreatedAt);
        b.Property(p => p.UpdatedAt);

        b.HasMany(p => p.Steps)
         .WithOne()
         .HasForeignKey(s => s.PlanId)
         .OnDelete(DeleteBehavior.Cascade);

        b.Ignore(p => p.DomainEvents);
    }
}

internal class StepConfiguration : IEntityTypeConfiguration<Step>
{
    public void Configure(EntityTypeBuilder<Step> b)
    {
        b.HasKey(s => s.Id);
        b.Property(s => s.Title)
         .HasConversion(t => t.Value, v => PlanTitle.Create(v))
         .HasMaxLength(200)
         .IsRequired();
        b.Property(s => s.Description).HasMaxLength(2000);
        b.Property(s => s.Order);
        b.Property(s => s.Status).HasConversion<string>();
        b.Property(s => s.CreatedAt);
    }
}
