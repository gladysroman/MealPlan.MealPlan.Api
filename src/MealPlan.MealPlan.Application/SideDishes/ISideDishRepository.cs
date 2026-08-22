using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.SideDishes;

public interface ISideDishRepository
{
    Task<List<SideDish>> GetAllAsync(CancellationToken cancellationToken);
    Task<SideDish?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<List<SideDish>> GetByMealIdAndUserIdAsync(string mealId, string userId, CancellationToken cancellationToken);
    Task<SideDish> AddAsync(SideDish sideDish, CancellationToken cancellationToken);
    Task<SideDish> UpdateAsync(SideDish sideDish, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
