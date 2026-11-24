using System.Security.Claims;

namespace MealPlan.MealPlan.Api.Auth;

public static class OwnershipExtensions
{
    // For routes that put a user id in the URL (such as /users/{userId}/meals): the id in the
    // URL must be the signed-in user's own, otherwise the request is refused with 403.
    public static TBuilder RequireSelf<TBuilder>(this TBuilder builder, string routeParameter = "userId")
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(async (context, next) =>
        {
            var signedInUserId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requestedUserId = context.HttpContext.Request.RouteValues[routeParameter]?.ToString();

            if (signedInUserId is null || !string.Equals(signedInUserId, requestedUserId, StringComparison.Ordinal))
            {
                return Results.Problem(
                    title: "You can only access your own data.",
                    statusCode: StatusCodes.Status403Forbidden);
            }

            return await next(context);
        });
}
