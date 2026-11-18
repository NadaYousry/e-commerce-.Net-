# Database Design Plan

## Implementation Design
1. Add the Identity user extension, Product, Order, OrderItem, and Payment entity classes.
2. Configure keys, constraints, indexes, decimal precision, relationships, enum serialization, and delete behavior with EF Core Fluent API.
3. Register a SQLite `DbContext` with ASP.NET Core dependency injection and configure Identity stores against it.
4. Add an initial EF Core migration and document the commands to create and apply it.
5. Add an idempotent development product seeder; invoke it after the database is migrated in the local startup workflow.
6. Add focused persistence tests for relationships, unique payment-per-order, seed repeatability, and order snapshots.

## Concept Notes for Implementation Session
- A C# entity is a class EF Core tracks; unlike a React view model, it can represent persisted state and relationships.
- `DbContext` is the unit of work: it tracks loaded/added entities and turns `SaveChangesAsync` into SQL operations.
- Navigation properties are object-level links; foreign-key columns are the relational representation.
- Explain migrations step by step: edit entity/configuration, generate with `dotnet ef migrations add <Name>`, inspect `Up` and `Down`, apply with `dotnet ef database update`, and verify `__EFMigrationsHistory`. Relate the migration to a reviewed, committed database change script rather than a runtime model reset.
- Async EF calls avoid blocking request threads while SQLite performs I/O.
- Identity persists account/security data and hashes/verifies passwords through its configured password hasher; it does not automatically mean a request is authorized.
- Authentication answers "who is calling?" Identity verifies credentials at sign-in, then the app issues a JWT. JWT bearer middleware validates that signed token on later requests and provides claims. Authorization checks roles/policies/resource ownership after identity is known.
- The browser's `localStorage` cart is only a request source: send `{ productId, quantity }` lines, address, and payment choice. The API must fetch current products and recalculate EGP prices/totals and stock availability; client values are not trusted.

## Validation
- Build the backend after model configuration.
- Apply migrations to a fresh SQLite file and verify the schema.
- Run tests against a temporary SQLite database, not the developer's persistent database.
- Start the API and confirm the sample product endpoint returns seeded data.
- Verify repeating the seeder does not duplicate products and that product updates do not alter historical order snapshots.
