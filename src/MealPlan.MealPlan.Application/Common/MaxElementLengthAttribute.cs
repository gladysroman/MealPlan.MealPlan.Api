using System.ComponentModel.DataAnnotations;

namespace MealPlan.MealPlan.Application.Common;

// [MaxLength] on a collection validates item COUNT, not the length of each item -- this
// validates that every string in the collection is at most maxLength characters.
public class MaxElementLengthAttribute(int maxLength) : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is not IEnumerable<string> items || items.All(item => item.Length <= maxLength);

    public override string FormatErrorMessage(string name) =>
        $"Each entry in {name} must be at most {maxLength} characters.";
}
