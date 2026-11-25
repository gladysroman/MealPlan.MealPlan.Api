using System.Security.Claims;
using MealPlan.MealPlan.Api.Auth;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class SideDishesEndpoints
{
    public static void MapSideDishesEndpoints(this WebApplication app)
    {
        app.MapPost("/sideDishes",
            async (CreateSideDishRequest request, ClaimsPrincipal user, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var sideDish = await service.AddSideDishAsync(request, user.GetUserId(), cancellationToken);
                return Results.Created($"/sideDishes/{sideDish.SideDishId}", ToDto(sideDish));
            });

        app.MapPut("/sideDishes/{sideDishId}",
            async (string sideDishId, UpdateSideDishRequest request, ClaimsPrincipal user, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var sideDish = await service.UpdateSideDishAsync(sideDishId, user.GetUserId(), request, cancellationToken);
                return sideDish is null ? Results.NotFound() : Results.Ok(ToDto(sideDish));
            });

        app.MapDelete("/sideDishes/{sideDishId}",
            async (string sideDishId, ClaimsPrincipal user, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.DeleteSideDishAsync(sideDishId, user.GetUserId(), cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            });
    }

    private static SideDishDto ToDto(SideDish sideDish) => new(
        sideDish.SideDishId,
        sideDish.MealId,
        sideDish.UserId,
        sideDish.SideDishName,
        sideDish.SideDishDescription,
        sideDish.CookingRecipe,
        sideDish.Ingredients.Select(ToIngredientDto).ToList(),
        sideDish.CreatedDate,
        sideDish.UpdatedDate);

    private static SideDishIngredientDto ToIngredientDto(SideDishIngredient ingredient) => new(
        ingredient.IngredientId,
        ingredient.IngredientName,
        ingredient.Allergens,
        ingredient.Amount,
        ingredient.Unit,
        ingredient.Calories,
        ingredient.CarbsG,
        ingredient.ProteinG,
        ingredient.FatG);
}
