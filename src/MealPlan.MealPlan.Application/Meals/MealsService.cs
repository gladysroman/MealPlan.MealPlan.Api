using MealPlan.MealPlan.Application.Common;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MealPlan.MealPlan.Application.Meals;

public class MealsService(
    IMealRepository repository,
    SideDishesService sideDishesService,
    IUnitOfWork unitOfWork,
    ILogger<MealsService> logger)
{
    private const int MaxAssignSideDishesAttempts = 4;
    private static readonly TimeSpan AssignSideDishesRetryDelay = TimeSpan.FromMilliseconds(250);

    public Task<List<Meal>> GetMealsAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<Meal?> GetMealByIdAsync(string id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<CreateMealResult> AddMealAsync(CreateMealRequest request, CancellationToken cancellationToken)
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

        var assignResult = await TryAssignSideDishesWithRetryAsync(request.SideDishIds, meal.MealId, cancellationToken);
        if (assignResult.MissingSideDishIds.Count > 0)
        {
            return new CreateMealResult(null, assignResult.MissingSideDishIds);
        }

        meal.MealName = request.MealName ?? GenerateMealName(assignResult.SideDishes);
        meal.MealDescription = request.MealDescription ?? GenerateMealDescription(assignResult.SideDishes);

        await repository.AddAsync(meal, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateMealResult(meal, []);
    }

    // Retries the side-dish existence check/stamp a few times, rechecking the full requested
    // list each attempt. Covers a timing race where POST /sideDishes calls are still committing
    // when POST /meals arrives, without treating a genuinely-missing id any differently.
    private async Task<TryAssignSideDishesResult> TryAssignSideDishesWithRetryAsync(List<string> sideDishIds, string mealId, CancellationToken cancellationToken)
    {
        var result = new TryAssignSideDishesResult([], []);

        for (var attempt = 1; attempt <= MaxAssignSideDishesAttempts; attempt++)
        {
            result = await sideDishesService.TryAssignSideDishesToMealAsync(sideDishIds, mealId, cancellationToken);
            if (result.MissingSideDishIds.Count == 0)
            {
                return result;
            }

            logger.LogWarning(
                "Attempt {Attempt}/{MaxAttempts} to assign side dishes to meal {MealId} found missing side dish ids: {MissingSideDishIds}",
                attempt, MaxAssignSideDishesAttempts, mealId, result.MissingSideDishIds);

            if (attempt < MaxAssignSideDishesAttempts)
            {
                await Task.Delay(AssignSideDishesRetryDelay, cancellationToken);
            }
        }

        logger.LogWarning(
            "Exhausted all {MaxAttempts} attempts to assign side dishes to meal {MealId}; still missing: {MissingSideDishIds}",
            MaxAssignSideDishesAttempts, mealId, result.MissingSideDishIds);

        return result;
    }

    // "A, B & C" -- comma between all but the last pair, "&" before the last.
    private static string GenerateMealName(IReadOnlyList<SideDish> sideDishes) =>
        sideDishes.Count == 0 ? "New Meal" : JoinAsEnglishList(sideDishes.Select(s => s.SideDishName).ToList());

    private static string GenerateMealDescription(IReadOnlyList<SideDish> sideDishes) =>
        sideDishes.Count == 0 ? "No side dishes yet." : string.Join(" ", sideDishes.Select(s => AsSentence(s.SideDishDescription)));

    // Capitalizes the first letter and ensures terminal punctuation, so descriptions read as
    // distinct sentences when joined regardless of how the caller typed them in.
    private static string AsSentence(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            return trimmed;
        }

        var capitalized = char.ToUpper(trimmed[0]) + trimmed[1..];
        return capitalized[^1] is '.' or '!' or '?' ? capitalized : capitalized + ".";
    }

    private static string JoinAsEnglishList(IReadOnlyList<string> items) =>
        items.Count <= 1
            ? items.Count == 1 ? items[0] : string.Empty
            : string.Join(", ", items.Take(items.Count - 1)) + " & " + items[^1];

    // Returns null when the meal doesn't exist (404). Otherwise a CreateMealResult: Meal null
    // means a newly-referenced side dish id doesn't exist (400); Meal set means success (200).
    public async Task<CreateMealResult?> UpdateMealAsync(string id, UpdateMealRequest request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        // No-orphan policy: side dishes dropped from this meal get deleted, not just detached.
        var removedSideDishIds = existing.SideDishIds.Except(request.SideDishIds).ToList();
        if (removedSideDishIds.Count > 0)
        {
            await sideDishesService.DeleteSideDishesAsync(removedSideDishIds, cancellationToken);
        }

        var assignResult = await TryAssignSideDishesWithRetryAsync(request.SideDishIds, id, cancellationToken);
        if (assignResult.MissingSideDishIds.Count > 0)
        {
            return new CreateMealResult(null, assignResult.MissingSideDishIds);
        }

        existing.MealName = request.MealName ?? GenerateMealName(assignResult.SideDishes);
        existing.MealDescription = request.MealDescription ?? GenerateMealDescription(assignResult.SideDishes);
        existing.SideDishIds = request.SideDishIds;
        existing.UpdatedDate = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateMealResult(updated, []);
    }

    public async Task<bool> DeleteMealAsync(string id, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        // No-orphan policy: deleting a meal cascades to delete all of its side dishes.
        if (existing.SideDishIds.Count > 0)
        {
            await sideDishesService.DeleteSideDishesAsync(existing.SideDishIds, cancellationToken);
        }

        await repository.DeleteAsync(id, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
