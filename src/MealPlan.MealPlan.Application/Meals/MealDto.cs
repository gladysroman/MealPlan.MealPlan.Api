using System.Text.Json.Serialization;

namespace MealPlan.MealPlan.Application.Meals;

public record MealDto(
    [property: JsonPropertyName("meal_id")] string MealId,
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("side_dish_ids")] IReadOnlyList<string> SideDishIds,
    [property: JsonPropertyName("liked_amount")] int LikedAmount,
    [property: JsonPropertyName("created_date")] DateTime CreatedDate,
    [property: JsonPropertyName("updated_date")] DateTime UpdatedDate);

public record CreateMealRequest(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("side_dish_ids")] List<string> SideDishIds);

public record UpdateMealRequest(
    [property: JsonPropertyName("side_dish_ids")] List<string> SideDishIds);
