using MealPlan.MealPlan.Domain.Entities;
using MealPlan.MealPlan.Infrastructure.Persistence;
using MealPlan.MealPlan.Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlan.MealPlan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<MealPlanDbContext>(options =>
            options.UseInMemoryDatabase("MealPlan"));

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }

    public static async Task SeedInfrastructureDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MealPlanDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await MealPlanSeeder.SeedAsync(dbContext, passwordHasher, configuration["Seed:DevPassword"], cancellationToken);
    }
}
