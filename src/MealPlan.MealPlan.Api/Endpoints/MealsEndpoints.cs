using MealPlan.MealPlan.Application.MealPlans;
using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Domain.Entities;

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

        // AddMeal — creates a meal from side dishes that already exist (typically just created via POST /sideDishes).
        app.MapPost("/meals",
            async (CreateMealRequest request, MealsService service, CancellationToken cancellationToken) =>
            {
                var result = await service.AddMealAsync(request, cancellationToken);
                if (result.Meal is null)
                {
                    return Results.BadRequest(new { missing_side_dish_ids = result.MissingSideDishIds });
                }

                return Results.Created($"/users/{result.Meal.UserId}/meals/{result.Meal.MealId}", ToDto(result.Meal));
            });
    }

    private static MealDto ToDto(Meal meal) => new(
        meal.MealId,
        meal.UserId,
        meal.MealName,
        meal.MealDescription,
        meal.SideDishIds,
        meal.LikedAmount,
        meal.CreatedDate,
        meal.UpdatedDate);
}
