using System.Security.Claims;
using MealPlan.MealPlan.Application.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

namespace MealPlan.MealPlan.Api.Auth;

public static class AuthenticationExtensions
{
    public const string CookieName = "mealplan.auth";

    private static readonly TimeSpan IdleTimeout = TimeSpan.FromHours(8);

    // Sliding alone would let an active session last forever, so sign-in also has a hard cap.
    private static readonly TimeSpan AbsoluteTimeout = TimeSpan.FromDays(7);

    // Where the sign-in time is kept inside the cookie. The cookie's own IssuedUtc can't be
    // used for the hard cap: it is reset every time sliding expiration renews the cookie.
    private const string SignedInAtKey = "signed_in_at";

    // The properties to sign in with. IsPersistent keeps the cookie across browser restarts,
    // for as long as the timeouts allow; without it the cookie would die with the browser window.
    public static AuthenticationProperties NewSessionProperties()
    {
        var properties = new AuthenticationProperties { IsPersistent = true };
        properties.Items[SignedInAtKey] = DateTimeOffset.UtcNow.ToString("O");
        return properties;
    }

    // A cookie with no readable sign-in time counts as expired.
    private static bool IsPastAbsoluteTimeout(AuthenticationProperties properties) =>
        !properties.Items.TryGetValue(SignedInAtKey, out var value)
        || !DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var signedInAt)
        || DateTimeOffset.UtcNow - signedInAt > AbsoluteTimeout;

    // The Ingredients API accepts this API's sign-in cookie. That only works while both use the
    // same application name, key folder, cookie name and scheme, so these values must match
    // Auth/AuthenticationExtensions.cs in MealPlan.Ingredients.Api.
    private const string DataProtectionApplicationName = "MealPlan";

    private static string DataProtectionKeysPath(IConfiguration configuration) =>
        configuration["DataProtection:KeysPath"]
        ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MealPlan",
            "DataProtectionKeys");

    public static IServiceCollection AddMealPlanAuthentication(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        services
            .AddDataProtection()
            .SetApplicationName(DataProtectionApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(DataProtectionKeysPath(configuration)));

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                // Plain http://localhost is used in development; everywhere else require https.
                options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;

                options.ExpireTimeSpan = IdleTimeout;
                options.SlidingExpiration = true;

                // This is an API: answer 401/403 instead of redirecting to a login page.
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };

                // Runs on every request that carries a cookie.
                options.Events.OnValidatePrincipal = async context =>
                {
                    if (IsPastAbsoluteTimeout(context.Properties))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync();
                        return;
                    }

                    // The cookie only proves who the user was at sign-in. Make sure that user
                    // still exists, so a deleted account loses access immediately.
                    var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    var users = context.HttpContext.RequestServices.GetRequiredService<UsersService>();
                    var user = userId is null
                        ? null
                        : await users.GetUserByIdAsync(userId, context.HttpContext.RequestAborted);
                    if (user is null)
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync();
                    }
                };
            });

        // Every endpoint requires a signed-in user unless it opts out with AllowAnonymous().
        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }
}
