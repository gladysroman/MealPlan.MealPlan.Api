using System.Security.Claims;

namespace MealPlan.MealPlan.Api.Auth;

public static class OwnershipExtensions
{
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
