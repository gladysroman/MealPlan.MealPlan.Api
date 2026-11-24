using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MealPlan.MealPlan.Application.Common;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.SideDishes;

public record SideDishDto(
    [property: JsonPropertyName("side_dish_id")] string SideDishId,
    [property: JsonPropertyName("meal_id")] string MealId,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("side_dish_name")] string SideDishName,
    [property: JsonPropertyName("side_dish_description")] string SideDishDescription,
    [property: JsonPropertyName("cooking_recipe")] string CookingRecipe,
    [property: JsonPropertyName("ingredients")] IReadOnlyList<SideDishIngredientDto> Ingredients,
    [property: JsonPropertyName("created_date")] DateTime CreatedDate,
    [property: JsonPropertyName("updated_date")] DateTime UpdatedDate);

public record SideDishIngredientDto(
    [property: JsonPropertyName("ingredient_id"), Required, MaxLength(100)] string IngredientId,
    [property: JsonPropertyName("ingredient_name"), Required, MaxLength(100)] string IngredientName,
    [property: JsonPropertyName("allergens"), Required, MaxElementLength(50)] IReadOnlyList<string> Allergens,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("unit"), Required(AllowEmptyStrings = true), MaxLength(20)] string Unit,
    [property: JsonPropertyName("calories")] int Calories,
    [property: JsonPropertyName("carbs_g")] decimal CarbsG,
    [property: JsonPropertyName("protein_g")] decimal ProteinG,
    [property: JsonPropertyName("fat_g")] decimal FatG);

// The owner is the signed-in user, so there is no user_id in the request.
// Every field but meal_id must be present; the description and recipe may be empty strings.
public record CreateSideDishRequest(
    [property: JsonPropertyName("meal_id")] string? MealId,
    [property: JsonPropertyName("side_dish_name"), Required, MaxLength(100)] string SideDishName,
    [property: JsonPropertyName("side_dish_description"), Required(AllowEmptyStrings = true), MaxLength(1000)] string SideDishDescription,
    [property: JsonPropertyName("cooking_recipe"), Required(AllowEmptyStrings = true), MaxLength(4000)] string CookingRecipe,
    [property: JsonPropertyName("ingredients"), Required] List<SideDishIngredientDto> Ingredients);

public record UpdateSideDishRequest(
    [property: JsonPropertyName("side_dish_name"), Required, MaxLength(100)] string SideDishName,
    [property: JsonPropertyName("side_dish_description"), Required(AllowEmptyStrings = true), MaxLength(1000)] string SideDishDescription,
    [property: JsonPropertyName("cooking_recipe"), Required(AllowEmptyStrings = true), MaxLength(4000)] string CookingRecipe,
    [property: JsonPropertyName("ingredients"), Required] List<SideDishIngredientDto> Ingredients);

public record TryAssignSideDishesResult(IReadOnlyList<string> MissingSideDishIds, IReadOnlyList<SideDish> SideDishes);
