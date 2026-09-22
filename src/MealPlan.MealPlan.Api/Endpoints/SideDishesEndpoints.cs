using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class SideDishesEndpoints
{
    public static void MapSideDishesEndpoints(this WebApplication app)
    {
        // AddSideDish — creates a side dish. MealId is optional so a side dish can be
        // created before the meal that will reference it exists.
        app.MapPost("/sideDishes",
            async (CreateSideDishRequest request, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var sideDish = await service.AddSideDishAsync(request, cancellationToken);
                return Results.Created($"/sideDishes/{sideDish.SideDishId}", ToDto(sideDish));
            });

        // UpdateSideDish — full replace of a side dish's editable fields.
        app.MapPut("/sideDishes/{id}",
            async (string id, UpdateSideDishRequest request, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var sideDish = await service.UpdateSideDishAsync(id, request, cancellationToken);
                return sideDish is null ? Results.NotFound() : Results.Ok(ToDto(sideDish));
            });

        // DeleteSideDish
        app.MapDelete("/sideDishes/{id}",
            async (string id, SideDishesService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.DeleteSideDishAsync(id, cancellationToken);
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
