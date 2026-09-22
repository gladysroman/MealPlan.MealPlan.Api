using MealPlan.MealPlan.Application.Common;
using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.SideDishes;

public class SideDishesService(ISideDishRepository repository, IUnitOfWork unitOfWork)
{
    public Task<List<SideDish>> GetSideDishesAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<SideDish?> GetSideDishByIdAsync(string id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<SideDish> AddSideDishAsync(CreateSideDishRequest request, CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow;
        var sideDish = new SideDish
        {
            SideDishId = $"sd_{Guid.NewGuid():N}",
            MealId = request.MealId ?? string.Empty,
            UserId = request.UserId,
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

    public async Task<SideDish?> UpdateSideDishAsync(string id, UpdateSideDishRequest request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
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

    public async Task<bool> DeleteSideDishAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return deleted;
    }

    // Checks that every requested side dish exists and, if so, stamps each with the meal's id
    // in the same pass (the entities came back tracked, so mutating them here is the "stamp").
    // Does NOT commit — this is meant to be staged as part of a larger unit of work (see
    // MealsService.AddMealAsync), alongside the meal insert itself.
    public async Task<TryAssignSideDishesResult> TryAssignSideDishesToMealAsync(List<string> sideDishIds, string mealId, CancellationToken cancellationToken)
    {
        var existingSideDishes = await repository.GetExistingSideDishesByIdsAsync(sideDishIds, cancellationToken);
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

    // Stages a delete for each id (no commit) — a building block for callers that need to
    // delete several side dishes as part of a larger unit of work (e.g. MealsService cascading
    // a meal delete, or removing side dishes dropped from a meal update).
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
