using MealPlan.MealPlan.Application.Common;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.SideDishes;

public class SideDishesService(ISideDishRepository repository, IUnitOfWork unitOfWork)
{
    public Task<List<SideDish>> GetSideDishesAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<SideDish> AddSideDishAsync(CreateSideDishRequest request, string userId, CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow;
        var sideDish = new SideDish
        {
            SideDishId = $"sd_{Guid.NewGuid():N}",
            MealId = request.MealId ?? string.Empty,
            UserId = userId,
            SideDishName = request.SideDishName,
            SideDishDescription = request.SideDishDescription,
            CookingRecipe = request.CookingRecipe,
            Ingredients = request.Ingredients.Select(ToEntity).ToList(),
            CreatedDate = date,
            UpdatedDate = date
        };

        var created = await repository.AddAsync(sideDish, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return created;
    }

    public async Task<SideDish?> UpdateSideDishAsync(string sideDishId, string userId, UpdateSideDishRequest request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetBySideDishIdAndUserIdAsync(sideDishId, userId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.SideDishName = request.SideDishName;
        existing.SideDishDescription = request.SideDishDescription;
        existing.CookingRecipe = request.CookingRecipe;
        existing.Ingredients = request.Ingredients.Select(ToEntity).ToList();
        existing.UpdatedDate = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return updated;
    }

    public async Task<bool> DeleteSideDishAsync(string sideDishId, string userId, CancellationToken cancellationToken)
    {
        var existing = await repository.GetBySideDishIdAndUserIdAsync(sideDishId, userId, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        var deleted = await repository.DeleteAsync(sideDishId, cancellationToken);
        if (deleted)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return deleted;
    }

    // Doesn't commit: the caller saves these changes together with the meal.
    public async Task<TryAssignSideDishesResult> TryAssignSideDishesToMealAsync(List<string> sideDishIds, string mealId, string userId, CancellationToken cancellationToken)
    {
        var existingSideDishes = await repository.GetBySideDishIdsAndUserIdAsync(sideDishIds, userId, cancellationToken);
        var existingIds = existingSideDishes.Select(s => s.SideDishId).ToHashSet();
        var missingIds = sideDishIds.Where(id => !existingIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            return new TryAssignSideDishesResult(missingIds, []);
        }

        var date = DateTime.UtcNow;
        foreach (var sideDish in existingSideDishes)
        {
            if (sideDish.MealId != mealId)
            {
                sideDish.MealId = mealId;
                sideDish.UpdatedDate = date;
            }
        }

        return new TryAssignSideDishesResult([], existingSideDishes);
    }

    // Doesn't commit: the caller saves the deletes together with its own changes.
    public async Task DeleteSideDishesAsync(IReadOnlyList<string> sideDishIds, CancellationToken cancellationToken)
    {
        foreach (var sideDishId in sideDishIds)
        {
            await repository.DeleteAsync(sideDishId, cancellationToken);
        }
    }

    private static SideDishIngredient ToEntity(SideDishIngredientDto dto) => new()
    {
        IngredientId = dto.IngredientId,
        IngredientName = dto.IngredientName,
        Allergens = dto.Allergens.ToList(),
        Amount = dto.Amount,
        Unit = dto.Unit,
        Calories = dto.Calories,
        CarbsG = dto.CarbsG,
        ProteinG = dto.ProteinG,
        FatG = dto.FatG
    };
}
