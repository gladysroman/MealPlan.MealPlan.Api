using MealPlan.MealPlan.Application.Users;
using MealPlan.MealPlan.Domain.Entities;
using MealPlan.MealPlan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlan.MealPlan.Repository;

public class UserRepository(MealPlanDbContext dbContext) : IUserRepository
{
    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Users.AsNoTracking().ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
