using System.Security.Claims;

namespace MealPlan.MealPlan.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The signed-in user has no user id claim.");
}
