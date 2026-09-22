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

        // UpdateMeal — full replace of name/description/side dishes. Side dishes dropped from
        // the list are deleted (no-orphan policy); newly-referenced ids are validated the same
        // way AddMeal validates them.
        app.MapPut("/meals/{id}",
            async (string id, UpdateMealRequest request, MealsService service, CancellationToken cancellationToken) =>
            {
                var result = await service.UpdateMealAsync(id, request, cancellationToken);
                if (result is null)
                {
                    return Results.NotFound();
                }

                if (result.Meal is null)
                {
                    return Results.BadRequest(new { missing_side_dish_ids = result.MissingSideDishIds });
                }

                return Results.Ok(ToDto(result.Meal));
            });

        // DeleteMeal — cascades to delete all of the meal's side dishes (no-orphan policy).
        app.MapDelete("/meals/{id}",
            async (string id, MealsService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.DeleteMealAsync(id, cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
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
