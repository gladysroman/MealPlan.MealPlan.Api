using System.Security.Claims;
using MealPlan.MealPlan.Api.Auth;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class SideDishesEndpoints
{
    public static void MapSideDishesEndpoints(this WebApplication app)
    {
        // AddSideDish — creates a side dish owned by the signed-in user. MealId is optional so a
        // side dish can be created before the meal that will reference it exists.
        app.MapPost("/sideDishes",
            async (CreateSideDishRequest request, ClaimsPrincipal user, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var sideDish = await service.AddSideDishAsync(request, user.GetUserId(), cancellationToken);
                return Results.Created($"/sideDishes/{sideDish.SideDishId}", ToDto(sideDish));
            });

        // UpdateSideDish — full replace of a side dish's editable fields.
        app.MapPut("/sideDishes/{id}",
            async (string id, UpdateSideDishRequest request, ClaimsPrincipal user, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var sideDish = await service.UpdateSideDishAsync(id, user.GetUserId(), request, cancellationToken);
                return sideDish is null ? Results.NotFound() : Results.Ok(ToDto(sideDish));
            });

        // DeleteSideDish
        app.MapDelete("/sideDishes/{id}",
            async (string id, ClaimsPrincipal user, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.DeleteSideDishAsync(id, user.GetUserId(), cancellationToken);
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
