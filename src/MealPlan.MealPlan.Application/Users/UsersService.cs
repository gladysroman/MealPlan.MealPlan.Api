using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.Users;

public class UsersService(IUserRepository repository)
{
    public Task<List<User>> GetUsersAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<User?> GetUserByIdAsync(string id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<User> AddUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow;
        var user = new User
        {
            UserId = $"usr_{Guid.NewGuid():N}",
            Name = request.Name,
            Email = request.Email,
            PrefUnits = request.PrefUnits,
            FriendIds = [],
            MealIds = [],
            CreatedDate = date,
            UpdatedDate = date
        };

        return await repository.AddAsync(user, cancellationToken);
    }

    public async Task<User?> UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Name = request.Name;
        existing.Email = request.Email;
        existing.PrefUnits = request.PrefUnits;
        existing.FriendIds = request.FriendIds;
        existing.MealIds = request.MealIds;
        existing.UpdatedDate = DateTime.UtcNow;

        return await repository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteUserAsync(string id, CancellationToken cancellationToken) =>
        repository.DeleteAsync(id, cancellationToken);
}
