using System.Text.Json.Serialization;

namespace MealPlan.MealPlan.Infrastructure.Persistence.SeedData;

internal sealed class UserSeedRecord
{
    [JsonPropertyName("user_id")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("pref_units")]
    public string PrefUnits { get; init; } = string.Empty;

    [JsonPropertyName("friend_ids")]
    public List<string> FriendIds { get; init; } = [];

    [JsonPropertyName("meal_ids")]
    public List<string> MealIds { get; init; } = [];

    [JsonPropertyName("created_date")]
    public DateTime CreatedDate { get; init; }

    [JsonPropertyName("updated_date")]
    public DateTime UpdatedDate { get; init; }
}
