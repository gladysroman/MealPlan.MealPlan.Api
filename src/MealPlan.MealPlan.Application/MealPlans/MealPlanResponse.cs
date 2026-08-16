using System.Text.Json.Serialization;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.MealPlans;

// Read-only composition of a meal + its side dishes for the full meal page.
// Built via the static From(...) factory so the assembly logic lives in one testable place.
public record MealPlanResponse(
    [property: JsonPropertyName("meal_id")] string MealId,
    [property: JsonPropertyName("meal_name")] string MealName,
    [property: JsonPropertyName("meal_description")] string MealDescription,
    [property: JsonPropertyName("prep_time_minutes")] int PrepTimeMinutes,
    [property: JsonPropertyName("cook_time_minutes")] int CookTimeMinutes,
    [property: JsonPropertyName("servings")] int Servings,
    [property: JsonPropertyName("nutrition_per_serving")] NutritionPerServingResponse NutritionPerServing,
    [property: JsonPropertyName("allergens")] IReadOnlyList<string> Allergens,
    [property: JsonPropertyName("side_dishes")] IReadOnlyList<SideDishResponse> SideDishes)
{
    public static MealPlanResponse From(Meal meal, IReadOnlyList<SideDish> sideDishes)
    {
        // Allergens for the page are the de-duplicated union across every side dish.
        var allergens = sideDishes
            .SelectMany(sideDish => sideDish.Allergens)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(allergen => allergen, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new MealPlanResponse(
            meal.MealId,
            meal.MealName,
            meal.MealDescription,
            meal.PrepTimeMinutes,
            meal.CookTimeMinutes,
            meal.Servings,
            NutritionPerServingResponse.From(sideDishes, meal.Servings),
            allergens,
            sideDishes.Select(SideDishResponse.From).ToList());
    }
}

public record NutritionPerServingResponse(
    [property: JsonPropertyName("calories")] int Calories,
    [property: JsonPropertyName("carbs_g")] decimal CarbsG,
    [property: JsonPropertyName("protein_g")] decimal ProteinG,
    [property: JsonPropertyName("fat_g")] decimal FatG)
{
    public static NutritionPerServingResponse From(IReadOnlyList<SideDish> sideDishes, int servings)
    {
        var ingredients = sideDishes.SelectMany(sideDish => sideDish.Ingredients).ToList();

        var totalCalories = ingredients.Sum(ingredient => ingredient.Calories);
        var totalCarbs = ingredients.Sum(ingredient => ingredient.CarbsG);
        var totalProtein = ingredients.Sum(ingredient => ingredient.ProteinG);
        var totalFat = ingredients.Sum(ingredient => ingredient.FatG);

        // Guard against a bad/zero serving count so we never divide by zero.
        var divisor = servings > 0 ? servings : 1;

        return new NutritionPerServingResponse(
            (int)Math.Round((decimal)totalCalories / divisor),
            Math.Round(totalCarbs / divisor, 1),
            Math.Round(totalProtein / divisor, 1),
            Math.Round(totalFat / divisor, 1));
    }
}

public record SideDishResponse(
    [property: JsonPropertyName("side_dish_id")] string SideDishId,
    [property: JsonPropertyName("side_dish_name")] string SideDishName,
    [property: JsonPropertyName("side_dish_description")] string SideDishDescription,
    [property: JsonPropertyName("cooking_recipe")] string CookingRecipe,
    [property: JsonPropertyName("allergens")] IReadOnlyList<string> Allergens,
    [property: JsonPropertyName("ingredients")] IReadOnlyList<SideDishIngredientDto> Ingredients)
{
    public static SideDishResponse From(SideDish sideDish) => new(
        sideDish.SideDishId,
        sideDish.SideDishName,
        sideDish.SideDishDescription,
        sideDish.CookingRecipe,
        sideDish.Allergens,
        sideDish.Ingredients.Select(ingredient => new SideDishIngredientDto(
            ingredient.IngredientId,
            ingredient.Name,
            ingredient.Amount,
            ingredient.Unit,
            ingredient.Calories,
            ingredient.CarbsG,
            ingredient.ProteinG,
            ingredient.FatG)).ToList());
}
