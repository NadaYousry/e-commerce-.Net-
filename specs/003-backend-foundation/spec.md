# Backend Foundation Specification

## Goal
Create the smallest runnable ASP.NET Core API foundation that can persist the approved e-commerce data model in SQLite. This step establishes project configuration and database lifecycle; product endpoints and authentication flows are separate follow-on features.

## Scope
- Target .NET 10 LTS and keep the SDK major version reproducible with `global.json`.
- Use one controller-based ASP.NET Core Web API project under `Backend/ECommerce.Api`.
- Use built-in OpenAPI support, RFC 7807 Problem Details error responses, and dependency injection.
- Configure EF Core SQLite and ASP.NET Core Identity persistence with a GUID user key.
- Implement the reviewed User, Product, Order, OrderItem, and Payment persistence model and constraints.
- Create and apply an initial EF Core migration; explain generated migration code and database application before running it.
- Seed a small repeatable catalog with EGP prices; do not seed a known-password user.

## Out of Scope
- Product HTTP endpoints, registration/login endpoints, JWT issuance/validation, React application, cart UI, checkout endpoint, payment flow, and deployment.
- No real card data or real payment processing.

## Acceptance Criteria
- The project targets `net10.0` and builds using the installed .NET 10 SDK.
- The API starts with configuration read from appsettings/development settings and secrets are not committed.
- EF Core uses SQLite and the Identity store uses the same configured database context.
- Database tables/constraints match `specs/002-database-design/data-model.md`, including separate `OrderStatus` and `PaymentStatus`, EGP currency codes, address snapshot fields, unique payment-per-order, restrictive history-preserving relationships, and decimal precision.
- Migrations are source-controlled artifacts; the initial migration applies to a fresh local database and is listed in `__EFMigrationsHistory`.
- Seed execution can be repeated without duplicate products and includes deterministic product values with `CurrencyCode = EGP`.
- API-wide errors use Problem Details; unexpected exceptions do not leak implementation details.
- OpenAPI is available in Development using the built-in .NET support; no Swashbuckle dependency is added.
- No application code is added outside this backend foundation slice.

## Learning Checkpoints
- Explain project entry point, service registration, middleware order, configuration, and how a controller API differs from React component routing.
- Explain `DbContext` as EF Core's unit of work and how entity classes, navigation properties, foreign keys, and Fluent API configuration map to relational tables.
- Before applying the migration, explain `dotnet ef migrations add`, inspect its `Up`/`Down` operations, apply with `dotnet ef database update`, and verify `__EFMigrationsHistory`.
- Relate ASP.NET Core dependency injection to passing dependencies via component props/Context while noting that the server container owns scoped lifetimes per request.