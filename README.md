# MealPlan.MealPlan.Api

Minimal API built on .NET 10 using Clean Architecture.

## Run

```
dotnet run --project src/MealPlan.MealPlan.Api
```

## Signing in during development

The seeded users in `users.json` have no password until you give them one. The password is
kept out of the repository with [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).
Set it once on your machine, then sign in as any seeded user with it:

```
dotnet user-secrets set "Seed:DevPassword" "<a password of your choice>" --project src/MealPlan.MealPlan.Api
```

Without it the API still runs, but nobody can sign in.
