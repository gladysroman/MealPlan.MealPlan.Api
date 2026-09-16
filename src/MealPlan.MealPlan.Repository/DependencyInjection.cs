using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Application.Users;
using Microsoft.Extensions.DependencyInjection;
using MealPlan.MealPlan.Application.Common;

namespace MealPlan.MealPlan.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMealRepository, MealRepository>();
        services.AddScoped<ISideDishRepository, SideDishRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
