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

    public Task<List<SideDish>> GetByMealAndUserAsync(string mealId, string userId, CancellationToken cancellationToken) =>
        dbContext.SideDishes.AsNoTracking()
            .Where(s => s.MealId == mealId && s.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<SideDish> AddAsync(SideDish sideDish, CancellationToken cancellationToken)
    {
        dbContext.SideDishes.Add(sideDish);
        await dbContext.SaveChangesAsync(cancellationToken);
        return sideDish;
    }

    public async Task<SideDish> UpdateAsync(SideDish sideDish, CancellationToken cancellationToken)
    {
        dbContext.SideDishes.Update(sideDish);
        await dbContext.SaveChangesAsync(cancellationToken);
        return sideDish;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var sideDish = await dbContext.SideDishes.FirstOrDefaultAsync(s => s.SideDishId == id, cancellationToken);
        if (sideDish is null)
        {
            return false;
        }

        dbContext.SideDishes.Remove(sideDish);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
