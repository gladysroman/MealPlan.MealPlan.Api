namespace MealPlan.MealPlan.Domain.Entities;

public class SideDish
{
    public string SideDishId { get; set; } = string.Empty;
    public string MealId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SideDishName { get; set; } = string.Empty;
    public string SideDishDescription { get; set; } = string.Empty;
    public string CookingRecipe { get; set; } = string.Empty;
    public List<SideDishIngredient> Ingredients { get; set; } = [];
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
