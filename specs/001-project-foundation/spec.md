# Project Foundation Specification

## Purpose
Build a small, working e-commerce POC that also teaches a React developer how an ASP.NET Core API, its database, and its request lifecycle fit together. Specifications and plans are reviewed before the corresponding implementation begins.

## Scope
The completed POC will provide:
- Account registration and login, with hashed passwords and bearer-token authentication.
- Product listing, search, and product details with availability.
- A shopping cart with quantity controls and stock-aware feedback.
- Authenticated checkout, order creation, and simulated Cash on Delivery or mock-card payment.
- Persisted orders and payment outcomes, and server-controlled stock updates.
- Seed products, automated tests for the critical flows, and local setup documentation.

Out of scope: real financial transactions, storing or transmitting real card details, shipping-carrier integration, tax calculation, discount codes, multi-currency pricing, production deployment, and administrator/product-management screens.

## User Stories
1. As a visitor, I can browse and search active products and inspect a product's description, price, and stock.
2. As a visitor, I can add available quantities to a cart, adjust them, and see an accurate subtotal.
3. As a visitor, I can register and log in; invalid credentials and duplicate emails produce useful errors.
4. As a signed-in customer, I can submit checkout, choose Cash on Delivery or a mock-card outcome, and see the resulting order and payment status.
5. As a customer, I cannot buy more than the current stock, and checkout totals come from backend product prices rather than browser-submitted prices.
6. As a learner, I can follow each feature's specification, design, tasks, tests, and beginner-friendly explanation before moving to the next feature.

## Acceptance Criteria
- Anonymous users can call product read endpoints; checkout and order details require authentication and ownership checks.
- Passwords are never stored as plain text. The API does not expose persistence entities directly.
- The frontend cart is convenient and persistent on the current browser, but the backend is authoritative for price and stock at checkout.
- Checkout validates all requested products and quantities, calculates totals on the server, and persists the order and payment outcome consistently.
- Order status and payment status are separate persisted fields with independently defined meanings. COD orders are confirmed while payment remains pending; successful mock-card orders are confirmed with payment succeeded; declined mock-card orders are cancelled with payment failed.
- Checkout/order responses expose `orderStatus` and `paymentStatus` as separate values; the frontend does not infer one from the other.
- The checkout payload contains product IDs and quantities, delivery address, payment method, and (in Development only) mock outcome. It never contains trusted prices, totals, stock values, user IDs, or payment status. The backend reloads products, validates active status and current stock, calculates EGP totals, and changes stock only for accepted orders.
- Checkout collects full name, phone number, country, city, street address, and optional building/apartment details; no shipping fee or tax is calculated.
- API validation and expected failures use appropriate HTTP status codes and a consistent error response.
- The application can be run locally with SQLite and seeded data using documented commands.
- Tests cover registration/login, product retrieval, cart behavior, order creation, stock validation, and both mock-payment outcomes.
- A feature is not implemented until its specification, plan, and tasks are reviewed; its API contract and data-model changes are documented before implementation.

## Approved Architecture Decisions
- Use .NET 10 LTS, the current long-term-support release as of this design, and JavaScript with React/Vite.
- Use SQLite for local development and EF Core migrations for schema changes.
- Use ASP.NET Core Identity for user management and password hashing, plus short-lived JWT bearer tokens for this API POC.
- Keep the token in `sessionStorage` for this browser-only learning app; keep the non-sensitive cart in `localStorage` through React Context. Explain the XSS trade-off and use an HttpOnly-cookie/BFF design for a production browser app.
- Keep the cart client-side for the POC. Revalidate product existence, active status, price, and stock on the server during checkout; never trust a client total.
- Provide a minimal shipping address at checkout so a COD order has a destination. Do not calculate shipping fees or taxes in this POC.
- Let a mock-card checkout explicitly choose a simulated success or decline in Development only. Never collect card numbers. The API rejects the simulation control outside Development.
- Use EGP (Egyptian Pound) as the application currency for product prices, order totals, payment amounts, API responses, and frontend display. Use decimal amounts; currency conversion and other currencies are out of scope.
- Use one API call to submit checkout and record the simulated outcome. This keeps the POC understandable; a real gateway would use a separate payment intent and verified server-to-server webhook.

## Approved Decisions
- Default and only supported currency: EGP.
- Collect the specified minimal delivery address, including phone number and optional building/apartment details.
- Offer Cash on Delivery and mock card; mock success/decline is selectable only in Development.
- Keep order status and payment status separate. Payment status describes the payment attempt; order status describes fulfillment/acceptance state.

## Learning Approach
Before implementing each backend feature, explain what it does and why, the relevant .NET concepts, frontend/backend responsibilities, request lifecycle, key code, a practical HTTP example, and common mistakes. Relate concepts to React/JavaScript where useful. Keep explanations scoped to the current feature. For authentication, explicitly distinguish authentication (who the caller is) from authorization (what that caller may access), explain Identity's user/password-hashing/sign-in responsibilities, and show that the API separately issues and validates JWTs. For persistence, walk through entity/configuration changes, creating and inspecting an EF Core migration, and applying it to SQLite. For checkout, show exactly which cart fields the browser sends and why server-side product/price/stock validation is authoritative.
