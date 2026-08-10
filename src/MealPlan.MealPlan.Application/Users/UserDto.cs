using System.Text.Json.Serialization;

namespace MealPlan.MealPlan.Application.Users;

public record UserDto(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("pref_units")] string PrefUnits,
    [property: JsonPropertyName("friend_ids")] IReadOnlyList<string> FriendIds,
    [property: JsonPropertyName("meal_ids")] IReadOnlyList<string> MealIds,
    [property: JsonPropertyName("created_date")] DateTime CreatedDate,
    [property: JsonPropertyName("updated_date")] DateTime UpdatedDate);

// Response for GetUserInfo — the user profile page's core fields only.
public record UserProfileDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("pref_units")] string PrefUnits);

public record CreateUserRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("pref_units")] string PrefUnits);

public record UpdateUserRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("pref_units")] string PrefUnits,
    [property: JsonPropertyName("friend_ids")] List<string> FriendIds,
    [property: JsonPropertyName("meal_ids")] List<string> MealIds);
