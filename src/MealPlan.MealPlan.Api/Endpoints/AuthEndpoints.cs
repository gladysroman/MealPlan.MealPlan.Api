using System.Security.Claims;
using MealPlan.MealPlan.Api.Auth;
using MealPlan.MealPlan.Application.Auth;
using MealPlan.MealPlan.Application.Users;
using MealPlan.MealPlan.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MealPlan.MealPlan.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/login",
            async (LoginRequest request, AuthService service, HttpContext httpContext, CancellationToken cancellationToken) =>
            {
                var user = await service.LoginAsync(request, cancellationToken);
                if (user is null)
                {
                    // Same answer for an unknown email and a wrong password.
                    return Results.Problem(
                        title: "Invalid email or password.",
                        statusCode: StatusCodes.Status401Unauthorized);
                }

                var identity = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, user.UserId),
                        new Claim(ClaimTypes.Name, user.Name)
                    ],
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    AuthenticationExtensions.NewSessionProperties());

                return Results.Ok(ToCurrentUserDto(user));
            })
            .AllowAnonymous();

        // Logout — clears the cookie. Safe to call when already signed out.
        auth.MapPost("/logout",
            async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.NoContent();
            })
            .AllowAnonymous();

        // Me — who is signed in. The UI calls this on page load to find out if a session exists.
        auth.MapGet("/me",
            async (ClaimsPrincipal principal, UsersService service, HttpContext httpContext, CancellationToken cancellationToken) =>
            {
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = userId is null ? null : await service.GetUserByIdAsync(userId, cancellationToken);
                if (user is null)
                {
                    // The cookie is valid but its user no longer exists, so end the session.
                    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return Results.Unauthorized();
                }

                return Results.Ok(ToCurrentUserDto(user));
            })
            .RequireAuthorization();
    }

    private static CurrentUserDto ToCurrentUserDto(User user) => new(
        user.UserId,
        user.Name,
        user.Email,
        user.PrefUnits);
}
