using MealPlan.MealPlan.Api.Auth;
using MealPlan.MealPlan.Api.Endpoints;
using MealPlan.MealPlan.Application.Auth;
using MealPlan.MealPlan.Application.MealPlans;
using MealPlan.MealPlan.Application.Meals;
using MealPlan.MealPlan.Application.SideDishes;
using MealPlan.MealPlan.Application.Users;
using MealPlan.MealPlan.Infrastructure;
using MealPlan.MealPlan.Repository;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMealPlanAuthentication(builder.Environment, builder.Configuration);
builder.Services.AddValidation();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure();
builder.Services.AddRepositories();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<MealsService>();
builder.Services.AddScoped<SideDishesService>();
builder.Services.AddScoped<MealPlansService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Everything requires sign-in by default; the development docs are opened up explicitly.
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).AllowAnonymous();
}

await app.Services.SeedInfrastructureDataAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapMealsEndpoints();
app.MapSideDishesEndpoints();

app.Run();
