using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Application.SideDishes;

namespace MealPlan.MealPlan.Application.MealPlans;

public class MealPlansService(IMealRepository mealRepository, ISideDishRepository sideDishRepository)
{
    // Returns the composed meal page, or null when no meal matches BOTH the meal id and the user id
    // (the endpoint turns that null into 204 No Content). A meal that exists but has no side dishes
    // still returns successfully, with an empty side-dish list.
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

    // Returns every meal page for the user, each composed with its own side dishes.
    // Side dishes are fetched once for the whole user and grouped by meal to avoid one query per meal.
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
