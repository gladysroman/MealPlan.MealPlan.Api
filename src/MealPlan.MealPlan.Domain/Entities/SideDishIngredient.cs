namespace MealPlan.MealPlan.Domain.Entities;

// Ingredient embedded directly in a side dish (no join to the ingredients catalogue).
// Key nutrition fields are copied in at write time.
public class SideDishIngredient
{
    public string IngredientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Calories { get; set; }
    public decimal CarbsG { get; set; }
    public decimal ProteinG { get; set; }
    public decimal FatG { get; set; }
}
