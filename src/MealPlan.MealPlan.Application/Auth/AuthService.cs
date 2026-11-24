using MealPlan.MealPlan.Application.Common;
using MealPlan.MealPlan.Application.Users;
using MealPlan.MealPlan.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MealPlan.MealPlan.Application.Auth;

public class AuthService(
    IUserRepository repository,
    IUnitOfWork unitOfWork,
    IPasswordHasher<User> passwordHasher)
{
    // Verified against when the email is unknown, so an unknown email takes as long as a wrong
    // password and the response time doesn't reveal which emails have accounts.
    private static string? _dummyHash;

    // Returns the user when the email and password match, otherwise null. The caller must not say
    // which of the two was wrong.
    public async Task<User?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await repository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            _dummyHash ??= passwordHasher.HashPassword(new User(), Guid.NewGuid().ToString("N"));
            passwordHasher.VerifyHashedPassword(new User(), _dummyHash, request.Password);
            return null;
        }

        // An empty hash means the account has no password; VerifyHashedPassword would throw on it.
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // The stored hash uses outdated settings; upgrade it now that we hold the real password.
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await repository.UpdateAsync(user, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return user;
    }
}
