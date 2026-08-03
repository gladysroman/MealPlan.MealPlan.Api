using MealPlan.MealPlan.Infrastructure.Persistence;
using MealPlan.MealPlan.Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlan.MealPlan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<MealPlanDbContext>(options =>
            options.UseInMemoryDatabase("MealPlan"));

        return services;
    }

    public static async Task SeedInfrastructureDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MealPlanDbContext>();
        await MealPlanSeeder.SeedAsync(dbContext, cancellationToken);
    }
}
