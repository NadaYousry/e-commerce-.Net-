# Data Model

## Entity Relationship Diagram

```mermaid
erDiagram
    USER ||--o{ ORDER : places
    ORDER ||--|{ ORDER_ITEM : contains
    PRODUCT ||--o{ ORDER_ITEM : referenced_by
    ORDER ||--|| PAYMENT : has

    USER {
        guid Id PK
        string Email UK
        string PasswordHash
        string DisplayName
    }
    PRODUCT {
        int Id PK
        string Name
        string Description
        decimal Price
        string CurrencyCode
        int StockQuantity
        string ImageUrl
        bool IsActive
    }
    ORDER {
        int Id PK
        guid UserId FK
        datetime CreatedAtUtc
        string OrderStatus
        string CurrencyCode
        decimal Subtotal
        string FullName
        string PhoneNumber
        string Country
        string StreetAddress
        string BuildingApartmentDetails
        string City
    }
    ORDER_ITEM {
        int Id PK
        int OrderId FK
        int ProductId FK
        string ProductName
        decimal UnitPrice
        int Quantity
        decimal LineTotal
    }
    PAYMENT {
        int Id PK
        int OrderId FK_UK
        string PaymentMethod
        string PaymentStatus
        decimal Amount
        string CurrencyCode
        datetime CreatedAtUtc
    }
```

## Entities and Relationships

### User
Use Identity's user entity extended with a display name. Identity owns credential storage, password hashing, normalized email, and related account security fields. The application never stores a raw password. The unique normalized email prevents case-variant duplicate registrations.

### Product
Catalog row with an integer primary key, descriptive fields, fixed-precision EGP price, stock count, image URL, and active flag. Only active products are returned publicly and accepted for checkout. Use non-negative price and stock validation. Product DTOs expose currency code `EGP` so the frontend formats amounts consistently.

### Order
Belongs to exactly one user; a user can place many orders (one-to-many). Stores creation time in UTC, server-computed subtotal, currency code `EGP`, order lifecycle status, and a snapshot of full name, phone number, country, city, street address, and optional building/apartment details. The authenticated identity claim supplies `UserId`; request JSON cannot set it.

### OrderItem
Belongs to one order; each order has one or more items (one-to-many). References a product (many order items may refer to a product) and snapshots product name and unit price at purchase time. `LineTotal` is computed by the server as unit price times quantity. Product deletion is restricted while order items reference it; deactivation is preferred.

### Payment
One payment record per order in this POC (one-to-one enforced by a unique `OrderId`). Stores method, EGP amount, currency code, payment status, and creation time, but no card number or security code. A later real-payment design may need multiple attempts, provider references, idempotency keys, and webhook event records.

## Independent Status Fields
- `Order.OrderStatus` describes order acceptance/cancellation and is `Confirmed` or `Cancelled` in this POC.
- `Payment.PaymentStatus` describes the payment outcome and is `Pending`, `Succeeded`, or `Failed`.
- The fields are stored separately and both are returned to the frontend. COD maps to `Confirmed` + `Pending`; mock-card success maps to `Confirmed` + `Succeeded`; mock-card decline maps to `Cancelled` + `Failed`.

## Keys and EF Core Mapping
- A primary key uniquely identifies a row. A foreign key such as `Order.UserId` points to the related user and protects referential integrity.
- EF Core maps entity classes to tables and navigation properties to relationships. For example, an order's `Items` collection is the inverse navigation for `OrderItem.OrderId`.
- Use explicit Fluent API configuration for decimal precision, unique payment `OrderId`, indexes, required fields, enum storage, and delete behavior. This keeps database constraints visible and avoids relying on conventions for important rules.
- Use restrictive deletes for users/products with order history; do not make a product or user deletion erase financial/order records. Account deletion policy is outside the POC; no delete endpoint is planned.
- EF Core migrations are versioned schema changes. After an entity/configuration change, `dotnet ef migrations add <Name>` generates migration operations (`Up` applies the change, `Down` describes reversal); inspect and commit the migration, then `dotnet ef database update` applies pending changes to SQLite and records each applied migration in `__EFMigrationsHistory`. Do not silently call `EnsureCreated` in the application because it bypasses migration history. Later schema changes receive a new migration rather than rewriting an already-applied one.

## Seed Data
Seed a small set of deterministic products through a repeatable development seeder (stable identifiers or lookup keys and upsert/no-duplicate behavior). Include varied names, descriptions, EGP prices, image URLs, and stock values. Do not seed a default account with a known password. Keep product image URLs configurable or use reliable public placeholder/product assets documented in the frontend phase.

## Data Integrity Rules
- Quantities must be positive; stock must remain non-negative.
- Order item price, line total, subtotal, and payment amount are computed in EGP from persisted product prices by backend code. The frontend cart may contain stale or modified product details; checkout uses only product IDs and positive quantities from the submitted lines.
- The accepted checkout and stock update are persisted atomically. A failed mock-card attempt records a cancelled order and failed payment without reducing stock.
- Use decimal values (not binary floating point) for persisted currency values. The configured currency code is descriptive only; no exchange-rate conversion is performed.
