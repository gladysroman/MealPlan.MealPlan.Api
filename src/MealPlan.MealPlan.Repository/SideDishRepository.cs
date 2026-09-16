using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Domain.Entities;
using MealPlan.MealPlan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlan.MealPlan.Repository;

public class SideDishRepository(MealPlanDbContext dbContext) : ISideDishRepository
{
    public Task<List<SideDish>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.SideDishes.AsNoTracking().ToListAsync(cancellationToken);

    public Task<SideDish?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        dbContext.SideDishes.AsNoTracking().FirstOrDefaultAsync(s => s.SideDishId == id, cancellationToken);

    public Task<List<SideDish>> GetByMealIdAndUserIdAsync(string mealId, string userId, CancellationToken cancellationToken) =>
        dbContext.SideDishes.AsNoTracking()
            .Where(s => s.MealId == mealId && s.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<List<SideDish>> GetByUserIdAsync(string userId, CancellationToken cancellationToken) =>
        dbContext.SideDishes.AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<List<SideDish>> GetExistingSideDishesByIdsAsync(List<string> ids, CancellationToken cancellationToken) =>   
        dbContext.SideDishes
            .Where(s => ids.Contains(s.SideDishId))
            .ToListAsync(cancellationToken);

    public Task<SideDish> AddAsync(SideDish sideDish, CancellationToken cancellationToken)
    {
        dbContext.SideDishes.Add(sideDish);
        return Task.FromResult(sideDish);
    }

    public async Task<SideDish> UpdateAsync(SideDish sideDish, CancellationToken cancellationToken)
    {
        var existing = await dbContext.SideDishes.FirstAsync(s => s.SideDishId == sideDish.SideDishId, cancellationToken);

        dbContext.Entry(existing).CurrentValues.SetValues(sideDish);
        existing.Ingredients = sideDish.Ingredients;
        return existing;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var sideDish = await dbContext.SideDishes.FirstOrDefaultAsync(s => s.SideDishId == id, cancellationToken);
        if (sideDish is null)
        {
            return false;
        }

        dbContext.SideDishes.Remove(sideDish);
        return true;
    }
}
