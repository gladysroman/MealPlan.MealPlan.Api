namespace MealPlan.MealPlan.Application.Common;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
}