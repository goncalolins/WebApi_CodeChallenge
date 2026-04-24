# Rocket Store — Code Challenge Solution

REST API for managing products, customers and orders, implemented on **.NET 10** with EF Core (InMemory).

---

## Architecture

The solution is organised in five projects following a pragmatic **Clean Architecture** layout. The dependency direction points inward: presentation depends on application, application depends on domain, infrastructure implements application-defined abstractions.

```
RocketStore.slnx
├── WebApi/                    Presentation (controllers, middleware, composition root)
├── WebApi.Application/        Use cases, DTOs, validators, service/repository interfaces, mappings
├── WebApi.Domain/             Entities
├── WebApi.Infrastructure/     EF Core DbContext + concrete repositories
└── WebApi.Tests/              xUnit + FluentAssertions + EF InMemory
```

### Design decisions (and the reasoning behind them)

- **No generic `IRepository<T>` / Unit-of-Work wrapper.** EF Core's `DbContext` already is a UoW and each `DbSet<T>` already exposes repository-like semantics. Wrapping these in a generic abstraction adds ceremony, hides EF features (`AsNoTracking`, `Include`, projections) and makes testing harder, not easier. I use **specific repositories per aggregate** (`IProductRepository`, `ICustomerRepository`, `IOrderRepository`, plus narrow read-only lookups for cross-aggregate reads in `OrderService`). Each exposes only the operations its collaborators need.
- **Dependency inversion is explicit.** All interfaces live in `Application`; `Infrastructure` implements them. The API project depends on both layers only at the composition root (`Program.cs` → `AddApplication()` / `AddInfrastructure()`).
- **FluentValidation** drives request validation, not data annotations or hand-rolled validators. Validators live in the Application layer next to the DTOs they validate and are auto-registered by assembly scan.
- **Global exception middleware** translates domain-typed exceptions (`NotFoundException`, `ConflictException`, `InsufficientStockException`, `ValidationException`) into RFC 7807 `application/problem+json` responses with the appropriate status codes. Controllers stay thin — they never catch domain exceptions and never build error payloads by hand.
- **Manual mapping via extension methods** (`product.ToDto()`, `request.ToEntity()`), not AutoMapper/Mapster. For four aggregates the cost of a real mapping library outweighs the benefit; extension methods are explicit, discoverable and have zero runtime surprises.
- **DTOs as C# records.** Immutable by construction; request DTOs cannot be tampered with after model binding.
- **CancellationToken plumbed through** from controllers to repositories.

---

## What's implemented

### Mandatory (all tasks)

- **Products CRUD** (`ProductsController`) — all five endpoints, validation of `Name` and `Price > 0`.
- **Customers CRUD** (`CustomersController`) — all five endpoints, email format validation, `GET /{id}` includes the customer's orders, `DELETE` returns **409 Conflict** when the customer has existing orders.
- **Orders** (`OrdersController`) — list, get-by-id and place:
  - Rejects unknown `CustomerId` → **404**.
  - Rejects unknown `ProductId` → **404**.
  - Rejects `Quantity < 1` via validation → **400**.
  - **Snapshots `UnitPrice` from the product at the moment of purchase** (price changes afterwards do not affect historical orders — covered by test).
  - Decrements product stock by the ordered quantity.
  - Returns **422 Unprocessable Entity** when stock is insufficient, aggregating quantities of the same product across line items.
  - Sets `CreatedAt = DateTime.UtcNow` server-side.
- **Input validation** via FluentValidation, returning **400** with a field-level error dictionary.

### Bonus

| # | Bonus | Status |
|---|---|---|
| 1 | Pagination (`page`, `pageSize` on list endpoints) | Done — on products, customers and orders |
| 2 | Search / filter (`name` on products, `email` on customers) | Done — partial match, case-insensitive |
| 3 | Unit tests | Done — 17 tests, focused on `OrderService` (all business rules) |
| 4 | DTOs & mapping | Done — requests/responses are separate record types |
| 5 | Global error handling | Done — RFC 7807 `problem+json` responses |
| 6 | Structured logging | Done — `ILogger<T>` in services, structured placeholders for IDs/quantities |

---

## Running

```bash
dotnet restore RocketStore.slnx
dotnet run --project WebApi
```

The API listens on `http://localhost:5181` (and `https://localhost:7037` via the `https` launch profile). Swagger UI is available at `/swagger`, raw OpenAPI spec at `/openapi/v1.json`.

### Running the tests

```bash
dotnet test RocketStore.slnx
```

Each test uses an isolated EF InMemory database (unique name per test) so tests do not share state.

---

## Example requests

### Create a product

```http
POST /api/products
Content-Type: application/json

{
  "name": "Rocket",
  "description": "Fast ship",
  "price": 1200,
  "stock": 5
}
```

### Place an order

```http
POST /api/orders
Content-Type: application/json

{
  "customerId": 1,
  "items": [
    { "productId": 1, "quantity": 2 }
  ]
}
```

### Error response shape (RFC 7807)

```json
{
  "title": "Insufficient stock",
  "status": 422,
  "detail": "Insufficient stock for product 1. Requested: 999, available: 3.",
  "instance": "/api/orders",
  "errors": {}
}
```

Validation errors include a per-field `errors` dictionary:

```json
{
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Email": ["Email has an invalid format."]
  }
}
```

---

## Status codes

| Code | When |
|---|---|
| 200 OK | Successful `GET` / `PUT` |
| 201 Created | Successful `POST` (returns `Location` header) |
| 204 No Content | Successful `DELETE` |
| 400 Bad Request | Validation failure |
| 404 Not Found | Entity does not exist |
| 409 Conflict | Customer delete blocked by existing orders |
| 422 Unprocessable Entity | Insufficient stock on order placement |
| 500 Internal Server Error | Unhandled error (generic payload, details logged) |
