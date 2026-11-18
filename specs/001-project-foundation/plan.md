# Project Foundation Plan

## Architecture

```text
React + Vite browser app
  Pages / Router / Context
       | Axios + JSON + bearer token
       v
ASP.NET Core Web API (.NET 10)
  Controllers -> Application services -> EF Core DbContext
       |                                  |
       +-- Identity + JWT                  v
                                      SQLite database
```

### Frontend
- React and Vite with JavaScript, React Router DOM, Axios, and Context for authentication/cart state.
- Organize by responsibility (`api`, `components`, `pages`, `context`, `routes`, `styles`) without adding a state library for this POC.
- Store the cart in `localStorage`; keep the bearer token in `sessionStorage` as an explicit learning-POC compromise. Cart state can be edited by the user and is not authoritative. At checkout, Axios sends product IDs and quantities plus address/payment selections, never prices, totals, stock, user ID, or statuses. An Axios request interceptor attaches the token, and a response interceptor/central helper handles unauthorized and API validation errors.
- Display all product and checkout amounts as EGP, using currency information returned by the API and consistent formatting in the UI.
- Protect checkout routes and preserve the return location through login. Route guards improve the experience; API authorization remains the security boundary.

### Backend
- One ASP.NET Core Web API project with clear folders: `Controllers`, `Contracts` (request/response DTOs), `Services`, `Data`, `Entities`, and `Authentication`.
- Keep HTTP translation and status codes in controllers; place rules such as stock validation, order totals, and payment simulation in application services.
- Use constructor-injected interfaces for service dependencies where they clarify ownership or make testing easier. Avoid a generic repository layer: EF Core's `DbContext` already provides unit-of-work/repository behavior, and an extra layer would obscure the learning path in this small app.
- Use EF Core SQLite and migrations. Use ASP.NET Core Identity for account creation and password hashing, and JWT bearer authentication for API requests. Configure signing credentials outside source control using .NET user-secrets locally and environment configuration in deployment.
- Teach Identity and JWT as related but distinct components: Identity persists/manages users and verifies passwords; the application issues a signed JWT after successful sign-in; JWT bearer middleware validates that token on later requests and builds the caller's claims principal. Authentication identifies the caller; authorization policies/ownership checks decide what that caller may do.
- Configure CORS for the local Vite origin. Use HTTPS locally where practical, input validation, centralized exception handling, and structured logging without passwords/tokens.

## Why This Shape
A single backend project is less ceremony than a four-project Clean Architecture solution and makes the runtime path visible to a beginner. The folder boundaries still separate transport DTOs, business rules, persistence, and HTTP concerns. If the POC grows, those folders can become separate projects without changing the domain contracts first.

.NET 10 is selected because it is a stable LTS release with a longer support window than a short-term release. Pin the SDK with `global.json` during project setup and use matching EF Core 10 packages.

## Request Lifecycle

```text
React page -> Axios -> CORS/auth/validation middleware -> Controller
           -> application service -> EF Core DbContext -> SQLite
           <- response DTO + HTTP status <- controller <- service
```

For authenticated requests, JWT bearer middleware validates the token before the controller runs. The controller obtains the authenticated user identity from the request claims; the client does not choose the order owner. Explain authentication (identity verification) separately from authorization (permission/ownership checks), including why a valid JWT does not automatically grant access to every order.

## Checkout Transaction Outline
1. Require an authenticated customer and validate the request DTO, including address, payment method, positive quantities, and unique product IDs.
2. Read product IDs and quantities from the browser cart payload, load active products from the database, and compare requested quantities with current stock. Ignore any browser-supplied price, total, or stock field (the contract will not include them).
3. Recalculate unit prices and totals from database values in EGP; create order-item price/name snapshots and an order currency snapshot.
4. In one database transaction, create the order and payment record, set statuses from the selected method/outcome, and decrement stock only for an accepted order.
5. Keep `OrderStatus` and `PaymentStatus` as independent fields. For a simulated card decline, persist `Cancelled` order status and `Failed` payment status without changing stock. For COD, set `Confirmed` order status and `Pending` payment status, then decrement stock. For mock-card success, set `Confirmed` and `Succeeded`, then decrement stock.
6. Return an order response DTO. Never accept a client-supplied price, total, payment status, user ID, or production payment result as authoritative.

The checkout request will include cart lines as `{ productId, quantity }`, address fields (`fullName`, `phoneNumber`, `country`, `city`, `streetAddress`, optional `buildingApartmentDetails`), payment method, and a Development-only mock result for mock-card. The API contract is to be reviewed before checkout implementation. The exact concurrency strategy will be chosen during checkout implementation. SQLite serializes writes, but stock must still be rechecked within the transaction; tests should exercise competing/stale cart conditions.

## EF Core Migration Learning Path
Explain and demonstrate the lifecycle before the schema is applied: update entity/configuration code, run `dotnet ef migrations add <Name>` to generate a versioned schema change, inspect the generated `Up`/`Down` operations, then run `dotnet ef database update` to apply pending migrations and record them in EF Core's migrations history table. Explain that migrations are committed source artifacts and that `EnsureCreated` is not a substitute. When the schema changes later, create a new migration rather than editing an already-applied migration casually.

## API Surface (Indicative)
Exact request/response contracts are written and reviewed in the relevant feature spec before implementation.
- `POST /api/auth/register`, `POST /api/auth/login`
- `GET /api/products`, `GET /api/products/{id}`
- `POST /api/checkout` (authenticated)
- `GET /api/orders/{id}` (authenticated, owner-scoped)

Use DTOs rather than returning EF entities. Return `400` for invalid input, `401` for missing/invalid authentication, `403` for authenticated access that is not permitted, `404` for missing resources, and `409` for stock conflicts or duplicate account registration where appropriate. Use `201 Created` for successful registration/order creation and `200 OK` for reads/login.

## Validation Strategy
- Backend: xUnit tests, with API-level integration tests for auth and endpoint contracts; service-level tests for cart/order rules; a temporary SQLite database for persistence tests.
- Frontend: Vitest and React Testing Library for cart state, route protection, and API error/loading states.
- Manual end-to-end: run API and Vite app, register, browse, add to cart, sign in, and test COD, card success, card decline, and stale/out-of-stock checkout.
- Validate after each feature before starting the next. Keep sample data deterministic and document database reset/migration commands.

## Security and Production Notes
- JWTs are bearer credentials. `sessionStorage` limits lifetime to the tab session but remains accessible to injected JavaScript; production should consider a Backend-for-Frontend with Secure, HttpOnly, SameSite cookies and CSRF protections.
- A real payment provider should be called from the backend using a provider SDK. The browser should receive a hosted checkout/confirmation flow or client token, never secret credentials; the backend should verify signed webhooks and be idempotent before changing order/payment state. Never store card data.
- SQLite is appropriate for local POC setup, not necessarily multi-instance production operation. A production database and deployment configuration require a separate design review.
