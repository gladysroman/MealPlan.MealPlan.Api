using System.Text.Json.Serialization;

namespace MealPlan.MealPlan.Application.SideDishes;

public record SideDishDto(
    [property: JsonPropertyName("side_dish_id")] string SideDishId,
    [property: JsonPropertyName("meal_id")] string MealId,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("cooking_recipe")] string CookingRecipe,
    [property: JsonPropertyName("ingredients")] IReadOnlyList<SideDishIngredientDto> Ingredients,
    [property: JsonPropertyName("created_date")] DateTime CreatedDate,
    [property: JsonPropertyName("updated_date")] DateTime UpdatedDate);

public record SideDishIngredientDto(
    [property: JsonPropertyName("ingredient_id")] string IngredientId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("calories")] int Calories,
    [property: JsonPropertyName("carbs_g")] decimal CarbsG,
    [property: JsonPropertyName("protein_g")] decimal ProteinG,
    [property: JsonPropertyName("fat_g")] decimal FatG);

public record CreateSideDishRequest(
    [property: JsonPropertyName("meal_id")] string MealId,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("cooking_recipe")] string CookingRecipe,
    [property: JsonPropertyName("ingredients")] List<SideDishIngredientDto> Ingredients);

public record UpdateSideDishRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("cooking_recipe")] string CookingRecipe,
    [property: JsonPropertyName("ingredients")] List<SideDishIngredientDto> Ingredients);
