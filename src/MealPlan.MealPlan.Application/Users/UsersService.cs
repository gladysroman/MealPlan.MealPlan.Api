using MealPlan.MealPlan.Application.Common;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.Users;

public class UsersService(IUserRepository repository, IUnitOfWork unitOfWork)
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

        var created = await repository.AddAsync(user, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return created;
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

        var updated = await repository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return updated;
    }

    public async Task<bool> DeleteUserAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return deleted;
    }
}
