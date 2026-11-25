using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Application.SideDishes;

namespace MealPlan.MealPlan.Application.MealPlans;

public class MealPlansService(IMealRepository mealRepository, ISideDishRepository sideDishRepository)
{
    public async Task<MealPlanResponse?> GetMealPlanAsync(string userId, string mealId, CancellationToken cancellationToken)
    {
        var meal = await mealRepository.GetByMealIdAndUserIdAsync(mealId, userId, cancellationToken);
        if (meal is null)
        {
            return null;
        }

        var sideDishes = await sideDishRepository.GetByMealIdAndUserIdAsync(mealId, userId, cancellationToken);

        return MealPlanResponse.From(meal, sideDishes);
    }

    public async Task<List<MealPlanResponse>> GetAllMealPlansAsync(string userId, CancellationToken cancellationToken)
    {
        var meals = await mealRepository.GetByUserIdAsync(userId, cancellationToken);
        var sideDishes = await sideDishRepository.GetByUserIdAsync(userId, cancellationToken);
        var sideDishesByMealId = sideDishes.ToLookup(sideDish => sideDish.MealId);

        return meals
            .Select(meal => MealPlanResponse.From(meal, sideDishesByMealId[meal.MealId].ToList()))
            .ToList();
    }
}
