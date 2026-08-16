using System.Text.Json.Serialization;

namespace MealPlan.MealPlan.Infrastructure.Persistence.SeedData;

internal sealed class SideDishSeedRecord
{
    [JsonPropertyName("side_dish_id")]
    public string SideDishId { get; init; } = string.Empty;

    [JsonPropertyName("meal_id")]
    public string MealId { get; init; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("side_dish_name")]
    public string SideDishName { get; init; } = string.Empty;

    [JsonPropertyName("side_dish_description")]
    public string SideDishDescription { get; init; } = string.Empty;

    [JsonPropertyName("cooking_recipe")]
    public string CookingRecipe { get; init; } = string.Empty;

    [JsonPropertyName("allergens")]
    public List<string> Allergens { get; init; } = [];

    [JsonPropertyName("ingredients")]
    public List<SideDishIngredientSeedRecord> Ingredients { get; init; } = [];

    [JsonPropertyName("created_date")]
    public DateTime CreatedDate { get; init; }

    [JsonPropertyName("updated_date")]
    public DateTime UpdatedDate { get; init; }
}

internal sealed class SideDishIngredientSeedRecord
{
    [JsonPropertyName("ingredient_id")]
    public string IngredientId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("unit")]
    public string Unit { get; init; } = string.Empty;

    [JsonPropertyName("calories")]
    public int Calories { get; init; }

    [JsonPropertyName("carbs_g")]
    public decimal CarbsG { get; init; }

    [JsonPropertyName("protein_g")]
    public decimal ProteinG { get; init; }

    [JsonPropertyName("fat_g")]
    public decimal FatG { get; init; }
}
