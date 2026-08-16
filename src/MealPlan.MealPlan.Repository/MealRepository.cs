using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Domain.Entities;
using MealPlan.MealPlan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlan.MealPlan.Repository;

public class MealRepository(MealPlanDbContext dbContext) : IMealRepository
{
    public Task<List<Meal>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Meals.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Meal?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Meals.AsNoTracking().FirstOrDefaultAsync(m => m.MealId == id, cancellationToken);

    public Task<Meal?> GetByMealAndUserAsync(string mealId, string userId, CancellationToken cancellationToken) =>
        dbContext.Meals.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MealId == mealId && m.UserId == userId, cancellationToken);

    public async Task<Meal> AddAsync(Meal meal, CancellationToken cancellationToken)
    {
        dbContext.Meals.Add(meal);
        await dbContext.SaveChangesAsync(cancellationToken);
        return meal;
    }

    public async Task<Meal> UpdateAsync(Meal meal, CancellationToken cancellationToken)
    {
        dbContext.Meals.Update(meal);
        await dbContext.SaveChangesAsync(cancellationToken);
        return meal;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var meal = await dbContext.Meals.FirstOrDefaultAsync(m => m.MealId == id, cancellationToken);
        if (meal is null)
        {
            return false;
        }

        dbContext.Meals.Remove(meal);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
