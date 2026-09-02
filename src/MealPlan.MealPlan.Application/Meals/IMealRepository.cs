using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.Meals;

public interface IMealRepository
{
    Task<List<Meal>> GetAllAsync(CancellationToken cancellationToken);
    Task<Meal?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<Meal?> GetByMealIdAndUserIdAsync(string mealId, string userId, CancellationToken cancellationToken);
    Task<List<Meal>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<Meal> AddAsync(Meal meal, CancellationToken cancellationToken);
    Task<Meal> UpdateAsync(Meal meal, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
