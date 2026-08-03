namespace MealPlan.MealPlan.Domain.Entities;

public class Meal
{
    public string MealId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<string> SideDishIds { get; set; } = [];
    public int LikedAmount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    // Note: total nutrition is not stored on the meal. It is summed at read time
    // across the ingredients embedded in this meal's side dishes.
}
