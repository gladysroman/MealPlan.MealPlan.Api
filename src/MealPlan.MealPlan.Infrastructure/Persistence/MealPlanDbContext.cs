using MealPlan.MealPlan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealPlan.MealPlan.Infrastructure.Persistence;

public class MealPlanDbContext(DbContextOptions<MealPlanDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<SideDish> SideDishes => Set<SideDish>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.UserId);
            builder.PrimitiveCollection(u => u.FriendIds);
            builder.PrimitiveCollection(u => u.MealIds);
        });

        modelBuilder.Entity<Meal>(builder =>
        {
            builder.HasKey(m => m.MealId);
            builder.PrimitiveCollection(m => m.SideDishIds);
        });

        modelBuilder.Entity<SideDish>(builder =>
        {
            builder.HasKey(s => s.SideDishId);
            builder.OwnsMany(s => s.Ingredients, ingredient =>
            {
                ingredient.PrimitiveCollection(i => i.Allergens);
            });
        });
    }
}
