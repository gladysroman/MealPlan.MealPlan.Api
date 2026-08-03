using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Application.Users;
using MealPlan.MealPlan.Infrastructure;
using MealPlan.MealPlan.Repository;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure();
builder.Services.AddRepositories();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<MealsService>();
builder.Services.AddScoped<SideDishesService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

await app.Services.SeedInfrastructureDataAsync();

// Endpoints are intentionally not mapped yet — this is the bare-bones scaffold.

app.Run();
