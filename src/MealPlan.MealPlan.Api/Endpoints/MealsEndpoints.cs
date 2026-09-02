using MealPlan.MealPlan.Application.MealPlans;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class MealsEndpoints
{
    public static void MapMealsEndpoints(this WebApplication app)
    {
        // GetMealPlan — the full meal page: meal details composed with its side dishes.
        app.MapGet("/users/{userId}/meals/{mealId}",
            async (string userId, string mealId, MealPlansService service, CancellationToken cancellationToken) =>
            {
                var mealPlan = await service.GetMealPlanAsync(userId, mealId, cancellationToken);
                return mealPlan is null ? Results.NoContent() : Results.Ok(mealPlan);
            });

        // GetAllMealPlans — every meal page belonging to the user.
        app.MapGet("/users/{userId}/meals",
            async (string userId, MealPlansService service, CancellationToken cancellationToken) =>
            {
                var mealPlans = await service.GetAllMealPlansAsync(userId, cancellationToken);
                return Results.Ok(mealPlans);
            });
    }
}
