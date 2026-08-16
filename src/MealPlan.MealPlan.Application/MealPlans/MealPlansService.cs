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
        var meal = await mealRepository.GetByMealAndUserAsync(mealId, userId, cancellationToken);
        if (meal is null)
        {
            return null;
        }

        var sideDishes = await sideDishRepository.GetByMealAndUserAsync(mealId, userId, cancellationToken);

        return MealPlanResponse.From(meal, sideDishes);
    }
}
