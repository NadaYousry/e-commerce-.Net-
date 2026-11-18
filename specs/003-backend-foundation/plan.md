# Backend Foundation Plan

## Project Shape

```text
Backend/ECommerce.Api/
  Controllers/       HTTP endpoints (later feature slices)
  Data/              DbContext, EF configuration, development seeder
  Entities/          EF Core persistence models
  Middleware/        API-wide exception handling (only if customization is needed)
  Program.cs         dependency registrations and middleware pipeline
  appsettings*.json  non-secret settings such as the SQLite connection string
```

Use the controller-based Web API template because the approved architecture already names controllers and service boundaries. The API is one project for this POC; no extra Domain/Application/Infrastructure projects are needed yet.

## Dependencies and Configuration
- Target `net10.0`, pin the .NET 10 SDK using `global.json`, and use matching EF Core 10 packages.
- Use `Microsoft.EntityFrameworkCore.Sqlite` for SQLite persistence and `Microsoft.AspNetCore.Identity.EntityFrameworkCore` for Identity EF stores.
- Keep `Microsoft.EntityFrameworkCore.Design` as a development-time dependency for migration tooling; use a local `dotnet-ef` tool manifest so contributors get a matching major version.
- Configure SQLite via `ConnectionStrings:DefaultConnection`; put no credentials or signing secrets in committed configuration. JWT signing configuration is added in the later authentication feature.
- Use built-in `AddOpenApi`/`MapOpenApi` in Development rather than adding Swashbuckle.

## Persistence
- Extend `IdentityUser<Guid>` with the approved display-name field; use Identity's schema and password hasher rather than implementing credential storage.
- Add Product, Order, OrderItem, and Payment entities aligned with `specs/002-database-design/data-model.md`.
- Configure required fields, unique normalized email (Identity), unique payment `OrderId`, foreign keys/delete behavior, indexes, string enum persistence, and EGP currency defaults explicitly with Fluent API.
- Use decimal precision suitable for EGP amounts and calculate all order amounts in backend code. Avoid doing SQLite decimal range/order operations until their translation is verified.
- Add an idempotent development seeder with stable product lookup keys. Never create a sample user with a known password.

## API Pipeline
- Register controllers, built-in OpenAPI, EF Core context, Identity stores, and Problem Details services in `Program.cs` through dependency injection.
- Configure exception handling and status-code pages before mapping controllers, and enable OpenAPI only in Development.
- Do not add product or authentication routes in this foundation slice. Those features receive their own reviewed API contracts and tests.

## Migration Learning and Execution
1. Explain that EF Core compares the current model with a model snapshot to generate migration operations; migrations are code-reviewed schema changes, not automatic runtime synchronization.
2. Run `dotnet ef migrations add InitialCreate` after the entities/configuration are complete.
3. Inspect the generated `Up` and `Down` methods for Identity tables, keys, indexes, constraints, decimal columns, and relationships.
4. Apply with `dotnet ef database update`; EF creates/updates SQLite and records the migration in `__EFMigrationsHistory`.
5. Verify the fresh database and migration history. Future schema changes get new migrations; avoid `EnsureCreated` and avoid rewriting migrations already applied to shared databases.

## Validation
- Restore and build the API.
- Apply the initial migration to a fresh local SQLite file and inspect the resulting table/index shape.
- Start the API and confirm the built-in OpenAPI document is available in Development.
- Run the seeder twice and confirm product count remains stable and all seed products are EGP.
- Explain the project and request lifecycle before beginning product API work.