using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MealPlan.MealPlan.Application.Auth;

// The password limit is generous but bounded, so huge inputs can't be used to make hashing slow.
public record LoginRequest(
    [property: JsonPropertyName("email"), Required, MaxLength(254)] string Email,
    [property: JsonPropertyName("password"), Required, MaxLength(128)] string Password);

public record CurrentUserDto(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("pref_units")] string PrefUnits);
