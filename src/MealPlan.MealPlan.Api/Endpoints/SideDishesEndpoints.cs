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
