using System.Text.Json.Serialization;

namespace MealPlan.MealPlan.Infrastructure.Persistence.SeedData;

internal sealed class MealSeedRecord
{
    [JsonPropertyName("meal_id")]
    public string MealId { get; init; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("meal_name")]
    public string MealName { get; init; } = string.Empty;

    [JsonPropertyName("meal_description")]
    public string MealDescription { get; init; } = string.Empty;

    [JsonPropertyName("side_dish_ids")]
    public List<string> SideDishIds { get; init; } = [];

    [JsonPropertyName("prep_time_minutes")]
    public int PrepTimeMinutes { get; init; }

    [JsonPropertyName("cook_time_minutes")]
    public int CookTimeMinutes { get; init; }

    [JsonPropertyName("servings")]
    public int Servings { get; init; }

    [JsonPropertyName("liked_amount")]
    public int LikedAmount { get; init; }

    [JsonPropertyName("created_date")]
    public DateTime CreatedDate { get; init; }

    [JsonPropertyName("updated_date")]
    public DateTime UpdatedDate { get; init; }
}
