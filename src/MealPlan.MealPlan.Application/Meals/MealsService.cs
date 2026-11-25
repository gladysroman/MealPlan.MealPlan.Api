using System.Text;
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

    // Same limits as [MaxLength] on meal_name and meal_description in MealDto.cs.
    private const int MealNameMaxLength = 150;
    private const int MealDescriptionMaxLength = 1000;

    private const int DefaultServings = 1;

    public Task<List<Meal>> GetMealsAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<CreateMealResult> AddMealAsync(CreateMealRequest request, string userId, CancellationToken cancellationToken)
    {
        var sideDishIds = request.SideDishIds.Distinct().ToList();
        var date = DateTime.UtcNow;
        var meal = new Meal
        {
            MealId = $"meal_{Guid.NewGuid():N}",
            UserId = userId,
            SideDishIds = sideDishIds,
            PrepTimeMinutes = request.PrepTimeMinutes ?? 0,
            CookTimeMinutes = request.CookTimeMinutes ?? 0,
            Servings = request.Servings ?? DefaultServings,
            LikedAmount = 0,
            CreatedDate = date,
            UpdatedDate = date
        };

        var idsInOtherMeals = await FindSideDishIdsInOtherMealsAsync(sideDishIds, meal.MealId, userId, cancellationToken);
        if (idsInOtherMeals.Count > 0)
        {
            return new CreateMealResult(null, [], idsInOtherMeals);
        }

        var assignResult = await TryAssignSideDishesWithRetryAsync(sideDishIds, meal.MealId, userId, cancellationToken);
        if (assignResult.MissingSideDishIds.Count > 0)
        {
            return new CreateMealResult(null, assignResult.MissingSideDishIds, []);
        }

        meal.MealName = request.MealName ?? GenerateMealName(assignResult.SideDishes);
        meal.MealDescription = request.MealDescription ?? GenerateMealDescription(assignResult.SideDishes);

        await repository.AddAsync(meal, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateMealResult(meal, [], []);
    }

    // Deleting a meal deletes its side dishes, so a side dish can belong to only one meal.
    private async Task<List<string>> FindSideDishIdsInOtherMealsAsync(List<string> sideDishIds, string mealId, string userId, CancellationToken cancellationToken)
    {
        var meals = await repository.GetByUserIdAsync(userId, cancellationToken);
        var idsInOtherMeals = meals
            .Where(meal => meal.MealId != mealId)
            .SelectMany(meal => meal.SideDishIds)
            .ToHashSet();

        return sideDishIds.Where(idsInOtherMeals.Contains).ToList();
    }

    // POST /sideDishes calls may still be committing when POST /meals arrives.
    private async Task<TryAssignSideDishesResult> TryAssignSideDishesWithRetryAsync(List<string> sideDishIds, string mealId, string userId, CancellationToken cancellationToken)
    {
        var result = new TryAssignSideDishesResult([], []);

        for (var attempt = 1; attempt <= MaxAssignSideDishesAttempts; attempt++)
        {
            result = await sideDishesService.TryAssignSideDishesToMealAsync(sideDishIds, mealId, userId, cancellationToken);
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

    private static string GenerateMealName(IReadOnlyList<SideDish> sideDishes)
    {
        if (sideDishes.Count == 0)
        {
            return "New Meal";
        }

        var names = sideDishes.Select(s => s.SideDishName).ToList();
        while (names.Count > 1 && JoinAsEnglishList(names).Length > MealNameMaxLength)
        {
            names.RemoveAt(names.Count - 1);
        }

        var joined = JoinAsEnglishList(names);
        return joined.Length > MealNameMaxLength ? joined[..MealNameMaxLength] : joined;
    }

    private static string GenerateMealDescription(IReadOnlyList<SideDish> sideDishes)
    {
        if (sideDishes.Count == 0)
        {
            return "No side dishes yet.";
        }

        var sentences = sideDishes
            .Select(sideDish => AsSentence(sideDish.SideDishDescription))
            .Where(sentence => sentence.Length > 0)
            .ToList();
        if (sentences.Count == 0)
        {
            return "No description yet.";
        }

        var builder = new StringBuilder();
        foreach (var sentence in sentences)
        {
            var separatorLength = builder.Length > 0 ? 1 : 0;
            if (builder.Length + separatorLength + sentence.Length > MealDescriptionMaxLength)
            {
                break;
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(sentence);
        }

        if (builder.Length > 0)
        {
            return builder.ToString();
        }

        // One sentence can still be too long, because AsSentence may add a full stop.
        return sentences[0][..MealDescriptionMaxLength];
    }

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

    // Null means the meal was not found. A result with no Meal means a side dish problem.
    public async Task<CreateMealResult?> UpdateMealAsync(string mealId, string userId, UpdateMealRequest request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByMealIdAndUserIdAsync(mealId, userId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var sideDishIds = request.SideDishIds.Distinct().ToList();

        var idsInOtherMeals = await FindSideDishIdsInOtherMealsAsync(sideDishIds, mealId, userId, cancellationToken);
        if (idsInOtherMeals.Count > 0)
        {
            return new CreateMealResult(null, [], idsInOtherMeals);
        }

        // Side dishes dropped from the meal are deleted, not detached.
        var removedSideDishIds = existing.SideDishIds.Except(sideDishIds).ToList();
        if (removedSideDishIds.Count > 0)
        {
            await sideDishesService.DeleteSideDishesAsync(removedSideDishIds, cancellationToken);
        }

        var assignResult = await TryAssignSideDishesWithRetryAsync(sideDishIds, mealId, userId, cancellationToken);
        if (assignResult.MissingSideDishIds.Count > 0)
        {
            return new CreateMealResult(null, assignResult.MissingSideDishIds, []);
        }

        existing.MealName = request.MealName ?? GenerateMealName(assignResult.SideDishes);
        existing.MealDescription = request.MealDescription ?? GenerateMealDescription(assignResult.SideDishes);
        existing.SideDishIds = sideDishIds;
        existing.PrepTimeMinutes = request.PrepTimeMinutes ?? existing.PrepTimeMinutes;
        existing.CookTimeMinutes = request.CookTimeMinutes ?? existing.CookTimeMinutes;
        existing.Servings = request.Servings ?? existing.Servings;
        existing.UpdatedDate = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateMealResult(updated, [], []);
    }

    public async Task<bool> DeleteMealAsync(string mealId, string userId, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByMealIdAndUserIdAsync(mealId, userId, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        if (existing.SideDishIds.Count > 0)
        {
            await sideDishesService.DeleteSideDishesAsync(existing.SideDishIds, cancellationToken);
        }

        await repository.DeleteAsync(mealId, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
