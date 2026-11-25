namespace MealPlan.MealPlan.Domain.Entities;

// Copied from the ingredients catalogue when the side dish is saved; there is no join.
public class SideDishIngredient
{
    public string IngredientId { get; set; } = string.Empty;
    public string IngredientName { get; set; } = string.Empty;
    public List<string> Allergens { get; set; } = [];
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Calories { get; set; }
    public decimal CarbsG { get; set; }
    public decimal ProteinG { get; set; }
    public decimal FatG { get; set; }
}
