using Microsoft.EntityFrameworkCore;
using PlanManager.Application;
using PlanManager.Domain;
using PlanManager.GraphQL.Plans;
using PlanManager.GraphQL.Subscriptions;
using PlanManager.Infrastructure;
using PlanManager.Infrastructure.Persistence;
using PlanManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<PlanDbContext>(opt =>
    opt.UseSqlite("Data Source=plans.db")
       .EnableSensitiveDataLogging()
       .EnableDetailedErrors()
       .LogTo(Console.WriteLine, LogLevel.Warning));

builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

// ── Application (MediatR / CQRS) ─────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<PlanManager.Application.Plans.Commands.CreatePlanCommand>());

// ── GraphQL (HotChocolate) ────────────────────────────────────────────────────
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

// ── CORS (for Vue frontend) ───────────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// ── Migrate & seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlanDbContext>();
    await db.Database.MigrateAsync();
    Console.WriteLine(">>> DB path: " + db.Database.GetDbConnection().DataSource);
}

app.UseCors();
app.UseWebSockets();

// Wrap GraphQL with exception logging middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("=== UNHANDLED EXCEPTION ===");
        Console.WriteLine(ex.ToString());
        Console.ResetColor();
        throw;
    }
});

app.MapGraphQL();

app.Run();
