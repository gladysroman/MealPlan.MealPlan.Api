using MealPlan.MealPlan.Api.Auth;
using MealPlan.MealPlan.Application.Users;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("/users");

        users.MapGet("/{id}/profile", async (string id, UsersService service, CancellationToken cancellationToken) =>
        {
            var user = await service.GetUserByIdAsync(id, cancellationToken);
            return user is null ? Results.NoContent() : Results.Ok(ToProfileDto(user));
        })
        .RequireSelf("id");
    }

    private static UserProfileDto ToProfileDto(User user) => new(
        user.Name,
        user.Email,
        user.PrefUnits);
}
