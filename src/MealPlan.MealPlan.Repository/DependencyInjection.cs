using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlan.MealPlan.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMealRepository, MealRepository>();
        services.AddScoped<ISideDishRepository, SideDishRepository>();

        return services;
    }
}
