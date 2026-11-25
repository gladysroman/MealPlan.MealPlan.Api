using System.ComponentModel.DataAnnotations;

namespace MealPlan.MealPlan.Application.Common;

// [MaxLength] on a collection limits the item count; this limits the length of each item.
public class MaxElementLengthAttribute(int maxLength) : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is not IEnumerable<string> items || items.All(item => item.Length <= maxLength);

    public override string FormatErrorMessage(string name) =>
        $"Each entry in {name} must be at most {maxLength} characters.";
}
