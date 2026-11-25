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

    // The cookie's own IssuedUtc is reset on every sliding renewal, so the sign-in time is kept separately.
    private const string SignedInAtKey = "signed_in_at";

    public static AuthenticationProperties NewSessionProperties()
    {
        var properties = new AuthenticationProperties { IsPersistent = true };
        properties.Items[SignedInAtKey] = DateTimeOffset.UtcNow.ToString("O");
        return properties;
    }

    private static bool IsPastAbsoluteTimeout(AuthenticationProperties properties) =>
        !properties.Items.TryGetValue(SignedInAtKey, out var value)
        || !DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var signedInAt)
        || DateTimeOffset.UtcNow - signedInAt > AbsoluteTimeout;

    // Must match MealPlan.Ingredients.Api, which reads this cookie.
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

                options.Events.OnValidatePrincipal = async context =>
                {
                    if (IsPastAbsoluteTimeout(context.Properties))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync();
                        return;
                    }

                    // A deleted account loses access straight away.
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
