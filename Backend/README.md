# Backend Development

## Prerequisites
- .NET 10 SDK
- The repository-local EF CLI tool, installed from the manifest with `dotnet tool restore`

## Create or Update the Local Database

From the repository root:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project Backend/ECommerce.Api/ECommerce.Api.csproj
```

EF Core applies pending versioned migrations to `Backend/ECommerce.Api/ecommerce.db` and records applied migration IDs in `__EFMigrationsHistory`. Do not delete or edit an applied migration to change a database; create a new migration after model changes.

## Run the API

```powershell
dotnet run --project Backend/ECommerce.Api/ECommerce.Api.csproj
```

The API is configured to run with local development settings. In Development, built-in OpenAPI is available at `/openapi/v1.json`, and the first startup seeds three sample products if the product table is empty. Restarting the API does not duplicate those products. The API has no product or authentication endpoints yet; those are subsequent feature slices.

## EF Core Learning Loop

When a model changes, EF Core compares the current entity/configuration model with its model snapshot. Generate and inspect the resulting schema operations before applying them:

```powershell
dotnet tool run dotnet-ef migrations add DescriptiveChange --project Backend/ECommerce.Api/ECommerce.Api.csproj
dotnet tool run dotnet-ef database update --project Backend/ECommerce.Api/ECommerce.Api.csproj
```

`Up` describes how to move the database forward; `Down` describes a possible reversal. Migrations are source artifacts that should be reviewed and committed with the model changes. `EnsureCreated` is intentionally not used because it bypasses migration history.

## Current Backend Concepts
- `Program.cs` registers services in ASP.NET Core's dependency injection container and defines middleware order.
- `ECommerceDbContext` inherits from Identity's EF context, so one SQLite database stores accounts and e-commerce entities.
- `DbContext` is scoped per request and acts as a unit of work: it tracks entity changes and persists them through `SaveChangesAsync`.
- Identity handles user data and password hashing. Authentication/JWT issuance and authorization are specified for a later feature; no login route is available yet.
- All catalog, order, and payment amounts use EGP. The backend remains authoritative for future checkout price and stock calculations.