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

    // Must stay in sync with the [MaxLength] on MealName/MealDescription in MealDto.cs --
    // generated values are truncated to these same limits so they never violate the very
    // validation rule that bounds a user-supplied override.
    private const int MealNameMaxLength = 150;
    private const int MealDescriptionMaxLength = 1000;

    private const int DefaultServings = 1;

    public Task<List<Meal>> GetMealsAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<Meal?> GetMealByIdAsync(string id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

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

    // A side dish belongs to at most one meal: deleting a meal deletes its side dishes, so a
    // side dish shared by two meals would vanish from the other one. Returns the requested ids
    // that another of the user's meals already lists.
    private async Task<List<string>> FindSideDishIdsInOtherMealsAsync(List<string> sideDishIds, string mealId, string userId, CancellationToken cancellationToken)
    {
        var meals = await repository.GetByUserIdAsync(userId, cancellationToken);
        var idsInOtherMeals = meals
            .Where(meal => meal.MealId != mealId)
            .SelectMany(meal => meal.SideDishIds)
            .ToHashSet();

        return sideDishIds.Where(idsInOtherMeals.Contains).ToList();
    }

    // Retries the side-dish existence check/stamp a few times, rechecking the full requested
    // list each attempt. Covers a timing race where POST /sideDishes calls are still committing
    // when POST /meals arrives, without treating a genuinely-missing id any differently.
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

    // "A, B & C" -- comma between all but the last pair, "&" before the last. If the joined
    // name would exceed MealNameMaxLength, whole side dish names are dropped from the end
    // (not sliced mid-word) until it fits.
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

    // Joins each side dish's description as its own sentence, including only as many whole
    // sentences (not partial/mid-sentence slices) as fit within MealDescriptionMaxLength.
    private static string GenerateMealDescription(IReadOnlyList<SideDish> sideDishes)
    {
        if (sideDishes.Count == 0)
        {
            return "No side dishes yet.";
        }

        // Side dishes with a blank description contribute nothing.
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

        // Even the first sentence alone exceeds the limit (rare -- only possible because
        // AsSentence can add a trailing period) -- hard-truncate as a last resort so the
        // result never violates MealDescriptionMaxLength.
        return sentences[0][..MealDescriptionMaxLength];
    }

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
    // means a side dish id doesn't exist (400) or already belongs to another meal (409); Meal
    // set means success (200).
    // A meal that belongs to someone else is treated as not found (null), so the API doesn't
    // reveal whether another user's id exists.
    public async Task<CreateMealResult?> UpdateMealAsync(string id, string userId, UpdateMealRequest request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null || existing.UserId != userId)
        {
            return null;
        }

        var sideDishIds = request.SideDishIds.Distinct().ToList();

        var idsInOtherMeals = await FindSideDishIdsInOtherMealsAsync(sideDishIds, id, userId, cancellationToken);
        if (idsInOtherMeals.Count > 0)
        {
            return new CreateMealResult(null, [], idsInOtherMeals);
        }

        // No-orphan policy: side dishes dropped from this meal get deleted, not just detached.
        var removedSideDishIds = existing.SideDishIds.Except(sideDishIds).ToList();
        if (removedSideDishIds.Count > 0)
        {
            await sideDishesService.DeleteSideDishesAsync(removedSideDishIds, cancellationToken);
        }

        var assignResult = await TryAssignSideDishesWithRetryAsync(sideDishIds, id, userId, cancellationToken);
        if (assignResult.MissingSideDishIds.Count > 0)
        {
            return new CreateMealResult(null, assignResult.MissingSideDishIds, []);
        }

        existing.MealName = request.MealName ?? GenerateMealName(assignResult.SideDishes);
        existing.MealDescription = request.MealDescription ?? GenerateMealDescription(assignResult.SideDishes);
        existing.SideDishIds = sideDishIds;
        // Times and servings left out of the request keep their current values.
        existing.PrepTimeMinutes = request.PrepTimeMinutes ?? existing.PrepTimeMinutes;
        existing.CookTimeMinutes = request.CookTimeMinutes ?? existing.CookTimeMinutes;
        existing.Servings = request.Servings ?? existing.Servings;
        existing.UpdatedDate = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new CreateMealResult(updated, [], []);
    }

    // Someone else's meal is treated as not found (false), as in UpdateMealAsync.
    public async Task<bool> DeleteMealAsync(string id, string userId, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null || existing.UserId != userId)
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
