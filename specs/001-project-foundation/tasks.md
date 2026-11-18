# Project and Phase Tasks

## Phase 1: Foundation (Current Review)
- [x] Record scope, user stories, acceptance criteria, and approved design decisions.
- [x] Select a simple frontend/backend architecture and document trade-offs.
- [x] Define initial entities, relationships, and database integrity rules.
- [x] Break the work into reviewable feature phases.
- [x] Review and approve the foundation and database specifications.
- [x] Set EGP as the currency, specify checkout address fields, and approve Development-only mock-card outcomes plus COD.

## Phase 2: Backend Fundamentals
- [x] Specify the backend foundation setup, .NET version, configuration, middleware, persistence model, and migration workflow in `specs/003-backend-foundation/`.
- [x] Implement and validate the backend foundation using `specs/003-backend-foundation/tasks.md`.
- [ ] Explain EF Core migration creation, generated operations, inspection, application, and migration history before applying the initial schema.
- [ ] Implement API/SQLite/EF Core foundation and migrations; validate a fresh database.
- [ ] Specify, plan, and task the product API; implement DTOs, service, controller, seeding, and retrieval tests.
- [ ] Specify, plan, and task authentication; first explain authentication versus authorization, Identity user/password/sign-in responsibilities, and how the application issues JWTs validated by bearer middleware; then implement registration/login/JWT and security tests.
- [ ] Explain the backend request lifecycle and validate product and authentication flows before proceeding.

## Phase 3: Frontend Implementation
- [ ] Specify and review Vite/React structure, routing, CSS approach, and API error/loading conventions.
- [ ] Implement app shell and responsive navigation.
- [ ] Implement auth pages, token context, and protected routes with tests.
- [ ] Implement product listing, search, product detail, and stock-aware cart context with EGP prices.
- [ ] Validate cart behavior, including quantity bounds and persistence.

## Phase 4: Integration
- [ ] Specify and review API base URL, Axios configuration, CORS, token attachment, and error mapping.
- [ ] Integrate product/auth flows and verify browser-to-API behavior.
- [ ] Verify unauthenticated checkout redirects to login and restores the checkout destination.

## Phase 5: Checkout and Payment
- [ ] Specify and review checkout request/response, EGP amounts, required full name/phone/country/city/street address and optional building/apartment details, separate order/payment status transitions, transaction behavior, and payment simulation.
- [ ] Explain that the browser sends product IDs and quantities from its cart while the server reloads products and owns all price/stock validation and total calculation.
- [ ] Implement server-side price/stock validation, order snapshots, payment record, and atomic stock updates.
- [ ] Implement checkout summary/confirmation and COD/mock-card success/decline UI.
- [ ] Test ownership, stale stock, accepted order stock changes, failed payment, and repeat submissions.

## Phase 6: Validation and Learning Documentation
- [ ] Run backend, frontend, and end-to-end checks; record remaining limitations.
- [ ] Write root README with setup, migrations, seed data, tests, and run instructions.
- [ ] Document architecture, ERD, API examples, authentication flow, and checkout/payment flow.
- [ ] Add .NET glossary, React-to-backend concept comparisons, and end-to-end request lifecycle.
- [ ] Provide the phase summary and next step after each completed phase.

## Per-Feature Workflow Gate
Before implementation of any feature: write/review `spec.md`, clarify only necessary decisions, write `plan.md` and `tasks.md`, and add `data-model.md` or `api-contract.md` when relevant. After implementation, validate against acceptance criteria and update the docs to match behavior. Do not begin the next major feature until the current feature works and its concepts have been explained.
