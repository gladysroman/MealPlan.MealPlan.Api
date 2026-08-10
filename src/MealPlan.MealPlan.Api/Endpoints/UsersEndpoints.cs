using MealPlan.MealPlan.Application.Users;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("/users");

        // GetUserInfo — returns the profile page's core fields for a user.
        users.MapGet("/{id}/profile", async (string id, UsersService service, CancellationToken cancellationToken) =>
        {
            var user = await service.GetUserByIdAsync(id, cancellationToken);
            return user is null ? Results.NoContent() : Results.Ok(ToProfileDto(user));
        });
    }

    private static UserProfileDto ToProfileDto(User user) => new(
        user.Name,
        user.Email,
        user.PrefUnits);
}
