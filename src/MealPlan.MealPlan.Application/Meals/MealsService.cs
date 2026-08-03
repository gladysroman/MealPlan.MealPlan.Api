using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.Meals;

public class MealsService(IMealRepository repository)
{
    public Task<List<Meal>> GetMealsAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<Meal?> GetMealByIdAsync(string id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<Meal> AddMealAsync(CreateMealRequest request, CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow;
        var meal = new Meal
        {
            MealId = $"meal_{Guid.NewGuid():N}",
            UserId = request.UserId,
            SideDishIds = request.SideDishIds,
            LikedAmount = 0,
            CreatedDate = date,
            UpdatedDate = date
        };

        return await repository.AddAsync(meal, cancellationToken);
    }

    public async Task<Meal?> UpdateMealAsync(string id, UpdateMealRequest request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.SideDishIds = request.SideDishIds;
        existing.UpdatedDate = DateTime.UtcNow;

        return await repository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteMealAsync(string id, CancellationToken cancellationToken) =>
        repository.DeleteAsync(id, cancellationToken);
}
