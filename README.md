# E-Commerce (.NET + React)

An online store with an **ASP.NET Core Web API** backend and a **React** frontend. Prices are in Egyptian pounds (EGP).

**Shoppers** can:

- Browse products and open a product's details page
- Add products to a cart, which is saved in the browser
- Register, sign in, and check out with cash on delivery or a mock card payment
- View their past orders and each order's details

**Admins** get a **Dashboard** with two sections:

- **Manage products:** add, edit, deactivate and delete products
- **Manage users:** add users and give them the **Admin** or **User** role

---

## Tech stack

### Backend (`Backend/`)

| Area | Tool |
|---|---|
| Framework | .NET 10, ASP.NET Core Web API (controllers) |
| Database | SQLite (a single file, `ecommerce.db`) |
| ORM and migrations | Entity Framework Core 10, `dotnet-ef` (local tool) |
| Users and roles | ASP.NET Core Identity |
| Authentication | JWT bearer tokens |
| API docs | OpenAPI and Swagger UI (Development only) |
| Validation | DataAnnotations; errors are returned as ProblemDetails |
| Tests | xUnit, with an in-memory SQLite database per test; coverlet for coverage |

### Frontend (`frontend/`)

| Area | Tool |
|---|---|
| UI | React 19 |
| Build and dev server | Vite 8 with `@vitejs/plugin-react` |
| Linting | oxlint |
| Styling | Plain CSS, no framework |
| Routing | A small custom router (`src/router.jsx`) built on the History API |
| State | React Context (`src/store.jsx`), saved to `localStorage` |
| HTTP | `fetch`, wrapped in `src/api.js` |
| Forms and validation | A custom `useForm` hook and the rules in `src/validation.js` |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download). The version is pinned in `global.json`.
- [Node.js](https://nodejs.org/) 20.19 or newer, or 22.12 or newer, which Vite 8 requires.

---

## Getting started

Run the backend and the frontend in two separate terminals. Start both from the repository root.

### 1. Run the backend API

```bash
# Install the EF Core CLI listed in dotnet-tools.json
dotnet tool restore

# Create or update the SQLite database (only needed the first time, and after new migrations)
dotnet tool run dotnet-ef database update --project Backend/ECommerce.Api/ECommerce.Api.csproj

# Start the API
dotnet run --project Backend/ECommerce.Api/ECommerce.Api.csproj
```

- The API runs at **http://localhost:5249**.
- Swagger UI is at **http://localhost:5249/swagger**. To call protected endpoints, click **Authorize** and paste a token from `POST /api/auth/login`.
- In Development, the first startup adds sample products and a default admin account.

### 2. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

- The app runs at **http://localhost:5173**.
- The frontend calls the API at `http://localhost:5249`, which is set in `frontend/src/api.js`. The API only accepts requests from `http://localhost:5173` (its CORS setting), so keep both default ports.

### 3. Sign in as admin

| Email | Password |
|---|---|
| `admin@ecommerce.local` | `Admin@12345` |

This account is set in `Backend/ECommerce.Api/appsettings.Development.json` and is created only in Development. After signing in, open **Dashboard** in the top bar.

> These credentials and the JWT key in `appsettings.json` are for local development only. Replace them before deploying anywhere.

---

## Other commands

| Task | Command (from the repository root) |
|---|---|
| Run backend tests | `dotnet test Backend/ECommerce.Api.Tests` |
| Add an EF Core migration | `dotnet tool run dotnet-ef migrations add <Name> --project Backend/ECommerce.Api/ECommerce.Api.csproj` |
| Lint the frontend | `cd frontend && npm run lint` |
| Production build of the frontend | `cd frontend && npm run build` (output goes to `frontend/dist`) |
| Preview the production build | `cd frontend && npm run preview` |

---

## API overview

| Method | Endpoint | Access |
|---|---|---|
| `POST` | `/api/auth/register`, `/api/auth/login` | Public |
| `GET` | `/api/products`, `/api/products/{id}` | Public (active products only) |
| `POST` | `/api/checkout` | Signed-in user |
| `GET` | `/api/orders`, `/api/orders/{id}` | Signed-in user (own orders only) |
| `GET` `POST` `PUT` `DELETE` | `/api/admin/products` | Admin |
| `GET` `POST` | `/api/admin/users` | Admin |
| `PUT` | `/api/admin/users/{id}/role` | Admin |

A user's role is stored in their login token, so a role change takes effect the next time they sign in. Admins can't remove their own Admin role.

---

## Project structure

```
Backend/
  ECommerce.Api/            ASP.NET Core Web API
    Controllers/            Auth, products, checkout/orders, admin products, admin users
    Data/                   EF Core DbContext (Identity + store tables)
    Entities/               Product, Order, OrderItem, Payment, ApplicationUser, roles
    Migrations/             EF Core migrations
    Seeding/                Sample products and admin account (Development only)
    Services/               Auth, JWT, checkout, validation helpers
  ECommerce.Api.Tests/      xUnit tests
frontend/
  src/
    components/             Layout, AdminDashboard, RequireAdmin, FormField
    pages/                  Store, cart, checkout, orders, auth, admin pages
    api.js  router.jsx  store.jsx  useForm.js  validation.js
specs/                      Feature specs, plans and task lists
```
