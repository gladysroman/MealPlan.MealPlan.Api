using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.SideDishes;

public interface ISideDishRepository
{
    Task<List<SideDish>> GetAllAsync(CancellationToken cancellationToken);
    Task<SideDish?> GetBySideDishIdAndUserIdAsync(string sideDishId, string userId, CancellationToken cancellationToken);
    Task<List<SideDish>> GetBySideDishIdsAndUserIdAsync(List<string> sideDishIds, string userId, CancellationToken cancellationToken);
    Task<List<SideDish>> GetByMealIdAndUserIdAsync(string mealId, string userId, CancellationToken cancellationToken);
    Task<List<SideDish>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<SideDish> AddAsync(SideDish sideDish, CancellationToken cancellationToken);
    Task<SideDish> UpdateAsync(SideDish sideDish, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string sideDishId, CancellationToken cancellationToken);
}
