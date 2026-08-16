using MealPlan.MealPlan.Domain.Entities;

namespace MealPlan.MealPlan.Application.SideDishes;

public class SideDishesService(ISideDishRepository repository)
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
            MealId = request.MealId,
            UserId = request.UserId,
            SideDishName = request.SideDishName,
            SideDishDescription = request.SideDishDescription,
            CookingRecipe = request.CookingRecipe,
            Ingredients = request.Ingredients.Select(ToEntity).ToList(),
            CreatedDate = date,
            UpdatedDate = date
        };

        return await repository.AddAsync(sideDish, cancellationToken);
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

        return await repository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteSideDishAsync(string id, CancellationToken cancellationToken) =>
        repository.DeleteAsync(id, cancellationToken);

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
