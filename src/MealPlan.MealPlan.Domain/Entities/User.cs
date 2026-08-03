namespace MealPlan.MealPlan.Domain.Entities;

public class User
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrefUnits { get; set; } = string.Empty;
    public List<string> FriendIds { get; set; } = [];
    public List<string> MealIds { get; set; } = [];
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
