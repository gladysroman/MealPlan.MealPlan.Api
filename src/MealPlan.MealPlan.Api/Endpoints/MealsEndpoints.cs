using System.Security.Claims;
using MealPlan.MealPlan.Api.Auth;
using MealPlan.MealPlan.Application.MealPlans;
using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class MealsEndpoints
{
    public static void MapMealsEndpoints(this WebApplication app)
    {
        app.MapGet("/users/{userId}/meals/{mealId}",
            async (string userId, string mealId, MealPlansService service, CancellationToken cancellationToken) =>
            {
                var mealPlan = await service.GetMealPlanAsync(userId, mealId, cancellationToken);
                return mealPlan is null ? Results.NoContent() : Results.Ok(mealPlan);
            })
            .RequireSelf();

        app.MapGet("/users/{userId}/meals",
            async (string userId, MealPlansService service, CancellationToken cancellationToken) =>
            {
                var mealPlans = await service.GetAllMealPlansAsync(userId, cancellationToken);
                return Results.Ok(mealPlans);
            })
            .RequireSelf();

        app.MapPost("/meals",
            async (CreateMealRequest request, ClaimsPrincipal user, MealsService service, CancellationToken cancellationToken) =>
            {
                var result = await service.AddMealAsync(request, user.GetUserId(), cancellationToken);
                if (result.Meal is null)
                {
                    return SideDishProblem(result);
                }

                return Results.Created($"/users/{result.Meal.UserId}/meals/{result.Meal.MealId}", ToDto(result.Meal));
            });

        app.MapPut("/meals/{mealId}",
            async (string mealId, UpdateMealRequest request, ClaimsPrincipal user, MealsService service, CancellationToken cancellationToken) =>
            {
                var result = await service.UpdateMealAsync(mealId, user.GetUserId(), request, cancellationToken);
                if (result is null)
                {
                    return Results.NotFound();
                }

                if (result.Meal is null)
                {
                    return SideDishProblem(result);
                }

                return Results.Ok(ToDto(result.Meal));
            });

        app.MapDelete("/meals/{mealId}",
            async (string mealId, ClaimsPrincipal user, MealsService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.DeleteMealAsync(mealId, user.GetUserId(), cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            });
    }

    private static IResult SideDishProblem(CreateMealResult result) =>
        result.SideDishIdsInOtherMeals.Count > 0
            ? Results.Conflict(new { side_dish_ids_in_other_meals = result.SideDishIdsInOtherMeals })
            : Results.BadRequest(new { missing_side_dish_ids = result.MissingSideDishIds });

    private static MealDto ToDto(Meal meal) => new(
        meal.MealId,
        meal.UserId,
        meal.MealName,
        meal.MealDescription,
        meal.SideDishIds,
        meal.PrepTimeMinutes,
        meal.CookTimeMinutes,
        meal.Servings,
        meal.LikedAmount,
        meal.CreatedDate,
        meal.UpdatedDate);
}
