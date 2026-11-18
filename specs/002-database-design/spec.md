# Database Design Specification

## Goal
Define the smallest persistent model that supports customers, catalog browsing, order history, stock control, and the POC payment outcomes. This spec is for review; no database or application code is created in this phase.

## Requirements
- Persist users through ASP.NET Core Identity with a GUID primary key and unique normalized email.
- Persist products with price, stock, active status, and image URL.
- Persist orders with the owning user, status, item/total snapshots, and minimal delivery address.
- Persist order items with product reference plus name and unit-price snapshots at purchase time.
- Persist one payment outcome record per order for this POC.
- Preserve historical order details if a product's name or price changes. Do not cascade-delete purchased history when a product is removed.
- Store product, order, and payment amounts as decimal EGP values; persist `CurrencyCode = EGP` on products, orders, and payments for explicit API and historical currency context.
- Snapshot checkout full name, phone number, country, city, street address, and optional building/apartment details on the order.
- Keep order status and payment status in separate fields; define their meanings and allowed combinations independently.
- Seed deterministic sample catalog products for local development.

## Acceptance Criteria
- Relationships and deletion rules are represented in the data model document.
- The browser sends product IDs and quantities only for each cart line; the backend determines order ownership, prices, totals, active-product eligibility, and current stock.
- A checkout decline can be recorded without reducing product stock.
- Accepted card and COD orders decrease stock in the same transaction that persists the order.
- Database schema changes are made with EF Core migrations; development seed data is repeatable and does not create duplicates.

## Status Values
Use named C# enums (persisted as strings for readability) or validated constants; do not accept arbitrary client-provided status strings.
- Order status (order lifecycle): `Confirmed`, `Cancelled`
- Payment method: `CashOnDelivery`, `MockCard`
- Payment status (payment attempt): `Pending`, `Succeeded`, `Failed`

Order status and payment status are distinct persisted properties; one does not replace or encode the other. A COD order is `Confirmed` with payment `Pending`. A mock-card success is `Confirmed` with payment `Succeeded`. A mock-card decline is `Cancelled` with payment `Failed`. The declined order is retained for demonstration/auditability and does not reduce stock.

## Currency and Address
- EGP (Egyptian Pound) is the default and only supported currency in this POC. All product prices, order totals, payment amounts, API money values, and frontend displays are EGP; no conversion is performed.
- The order stores an address snapshot: required full name, phone number, country, city, and street address; building/apartment details are optional. These are delivery details, not a payment credential. No tax or shipping fee is calculated.
