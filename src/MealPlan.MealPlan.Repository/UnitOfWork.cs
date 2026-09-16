using MealPlan.MealPlan.Application.Common;
using MealPlan.MealPlan.Infrastructure.Persistence;

namespace MealPlan.MealPlan.Repository;

public class UnitOfWork(MealPlanDbContext dbContext) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}