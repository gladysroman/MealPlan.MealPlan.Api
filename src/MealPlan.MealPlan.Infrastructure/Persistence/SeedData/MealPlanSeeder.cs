using System.Text.Json;
using MealPlan.MealPlan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealPlan.MealPlan.Infrastructure.Persistence.SeedData;

internal static class MealPlanSeeder
{
    private static readonly string SeedDirectory =
        Path.Combine(AppContext.BaseDirectory, "Persistence", "SeedData");

    public static async Task SeedAsync(MealPlanDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedUsersAsync(dbContext, cancellationToken);
        await SeedMealsAsync(dbContext, cancellationToken);
        await SeedSideDishesAsync(dbContext, cancellationToken);
    }

    private static async Task SeedUsersAsync(MealPlanDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var records = await ReadSeedFileAsync<UserSeedRecord>("users.json", cancellationToken);

        var users = records.Select(record => new User
        {
            UserId = record.UserId,
            Name = record.Name,
            Email = record.Email,
            PrefUnits = record.PrefUnits,
            FriendIds = record.FriendIds,
            MealIds = record.MealIds,
            CreatedDate = record.CreatedDate,
            UpdatedDate = record.UpdatedDate
        });

        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMealsAsync(MealPlanDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Meals.AnyAsync(cancellationToken))
        {
            return;
        }

        var records = await ReadSeedFileAsync<MealSeedRecord>("meals.json", cancellationToken);

        var meals = records.Select(record => new Meal
        {
            MealId = record.MealId,
            UserId = record.UserId,
            SideDishIds = record.SideDishIds,
            LikedAmount = record.LikedAmount,
            CreatedDate = record.CreatedDate,
            UpdatedDate = record.UpdatedDate
        });

        dbContext.Meals.AddRange(meals);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSideDishesAsync(MealPlanDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.SideDishes.AnyAsync(cancellationToken))
        {
            return;
        }

        var records = await ReadSeedFileAsync<SideDishSeedRecord>("side_dishes.json", cancellationToken);

        var sideDishes = records.Select(record => new SideDish
        {
            SideDishId = record.SideDishId,
            MealId = record.MealId,
            UserId = record.UserId,
            Name = record.Name,
            Description = record.Description,
            CookingRecipe = record.CookingRecipe,
            Ingredients = record.Ingredients.Select(ingredient => new SideDishIngredient
            {
                IngredientId = ingredient.IngredientId,
                Name = ingredient.Name,
                Amount = ingredient.Amount,
                Unit = ingredient.Unit,
                Calories = ingredient.Calories,
                CarbsG = ingredient.CarbsG,
                ProteinG = ingredient.ProteinG,
                FatG = ingredient.FatG
            }).ToList(),
            CreatedDate = record.CreatedDate,
            UpdatedDate = record.UpdatedDate
        });

        dbContext.SideDishes.AddRange(sideDishes);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<List<T>> ReadSeedFileAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(SeedDirectory, fileName);
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, cancellationToken: cancellationToken) ?? [];
    }
}
