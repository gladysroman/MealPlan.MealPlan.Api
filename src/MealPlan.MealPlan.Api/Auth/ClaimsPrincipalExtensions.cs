using System.Security.Claims;

namespace MealPlan.MealPlan.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    // The id of the signed-in user, taken from the sign-in cookie. Only call this from
    // endpoints that require sign-in; it throws if the cookie somehow has no user id.
    public static string GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The signed-in user has no user id claim.");
}
