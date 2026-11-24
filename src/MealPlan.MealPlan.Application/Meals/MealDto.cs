using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.Meals;

public record MealDto(
    [property: JsonPropertyName("meal_id")] string MealId,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("meal_name")] string MealName,
    [property: JsonPropertyName("meal_description")] string MealDescription,
    [property: JsonPropertyName("side_dish_ids")] IReadOnlyList<string> SideDishIds,
    [property: JsonPropertyName("prep_time_minutes")] int PrepTimeMinutes,
    [property: JsonPropertyName("cook_time_minutes")] int CookTimeMinutes,
    [property: JsonPropertyName("servings")] int Servings,
    [property: JsonPropertyName("liked_amount")] int LikedAmount,
    [property: JsonPropertyName("created_date")] DateTime CreatedDate,
    [property: JsonPropertyName("updated_date")] DateTime UpdatedDate);

// The owner is the signed-in user, so there is no user_id in the request.
// Times and servings are optional: a new meal defaults to 0 minutes and 1 serving.
public record CreateMealRequest(
    [property: JsonPropertyName("meal_name"), MaxLength(150)] string? MealName,
    [property: JsonPropertyName("meal_description"), MaxLength(1000)] string? MealDescription,
    [property: JsonPropertyName("side_dish_ids"), Required, MaxLength(6)] List<string> SideDishIds,
    [property: JsonPropertyName("prep_time_minutes"), Range(0, 1440)] int? PrepTimeMinutes = null,
    [property: JsonPropertyName("cook_time_minutes"), Range(0, 1440)] int? CookTimeMinutes = null,
    [property: JsonPropertyName("servings"), Range(1, 100)] int? Servings = null);

public record UpdateMealRequest(
    [property: JsonPropertyName("meal_name"), MaxLength(150)] string? MealName,
    [property: JsonPropertyName("meal_description"), MaxLength(1000)] string? MealDescription,
    [property: JsonPropertyName("side_dish_ids"), Required, MaxLength(6)] List<string> SideDishIds,
    [property: JsonPropertyName("prep_time_minutes"), Range(0, 1440)] int? PrepTimeMinutes = null,
    [property: JsonPropertyName("cook_time_minutes"), Range(0, 1440)] int? CookTimeMinutes = null,
    [property: JsonPropertyName("servings"), Range(1, 100)] int? Servings = null);

// On failure Meal is null and exactly one of the id lists says why.
public record CreateMealResult(
    Meal? Meal,
    IReadOnlyList<string> MissingSideDishIds,
    IReadOnlyList<string> SideDishIdsInOtherMeals);
