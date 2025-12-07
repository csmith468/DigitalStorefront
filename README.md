# Digital Storefront

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.6-3178C6?logo=typescript)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?logo=microsoftazure)
![License](https://img.shields.io/badge/License-MIT-green)

A production-ready full-stack e-commerce admin console for managing a virtual pet collectibles catalog.

**Live Demo:** [digitalstorefront.dev](https://digitalstorefront.dev)

> **Try it now:** Click "Admin" then "Try It" to explore the full product management workflow without creating an account.

---

## Key Features

| Category | Highlights |
|----------|------------|
| **Backend** | Custom Dapper ORM, Result pattern, Polly resilience, multi-tier rate limiting, idempotency keys |
| **Frontend** | Custom component library (13 primitives), React Query, multi-layer error boundaries |
| **Security** | JWT + RBAC, SQL injection prevention, optimistic concurrency, correlation IDs |
| **Testing** | Testcontainers (real SQL Server), Vitest, Playwright E2E |
| **DevOps** | GitHub Actions CI/CD, Azure (App Service, Static Web Apps, SQL, Blob, Key Vault) |

---

## Overview

Digital Storefront is a cloud-native application with a React + TypeScript frontend and .NET 8 API backend, deployed to Azure with a full CI/CD pipeline.

### Why I Built This

I logged back into a childhood online game and found their website horribly dated, which made me want to rebuild it. I started planning a storefront, but realized a shopping cart wouldn't showcase much. So I pivoted to the admin console to build complex form workflows, image management, and the production patterns that don't show up in typical portfolio projects.

**What it does:**
- Admin console for product CRUD with multi-image management
- Public "Try" mode for testing the admin form without authentication
- Browse products by category and subcategory
- Search products with tag-based filtering

**Architecture at a glance:**

```text
┌─────────────────────────────────────────────────────────────────┐
│                     Azure Static Web Apps                       │
│              React 19 + TypeScript + React Query                │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Azure App Service                         │
│                 .NET 8 Core API + Dapper ORM                    │
│                              │                                  │
│           ┌──────────────────┼──────────────────┐               │
│           ▼                  ▼                  ▼               │
│      Azure SQL         Blob Storage       Key Vault             │
│      Database           (Images)          (Secrets)             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technologies |
|-------|--------------|
| Frontend | React 19, TypeScript, React Query, React Router v7, Tailwind CSS, Vite |
| Backend | .NET 8, Dapper (custom ORM abstraction), FluentValidation, Polly, Serilog |
| Database | SQL Server 2022, DbUp migrations |
| Testing | xUnit, Testcontainers, Vitest, Playwright |
| Cloud | Azure (App Service, Static Web Apps, SQL Database, Blob Storage, Key Vault, Application Insights) |
| DevOps | GitHub Actions, Docker |

---

## Backend Architecture

### Custom Dapper ORM Abstraction

Built a lightweight ORM layer on top of Dapper with attribute-based mapping, eliminating Entity Framework overhead while maintaining type safety:

```csharp
[DbTable("Product", Schema = "dbo")]
public class Product
{
    [DbPrimaryKey]
    public int ProductId { get; set; }

    [DbColumn]
    public string Name { get; set; }

    [DbColumn]
    public decimal Price { get; set; }

    // Audit fields populated automatically via IAuditContext
    [DbColumn]
    public int CreatedBy { get; set; }

    [DbColumn]
    public DateTime CreatedAt { get; set; }
}
```

**Interface Segregation for Data Access:**

Data access is split into three focused interfaces, enabling compiler-enforced boundaries:

```csharp
// Read operations - services that only query can't accidentally mutate
public interface IQueryExecutor
{
    Task<T?> GetByIdAsync<T>(int id);
    Task<IEnumerable<T>> GetWhereAsync<T>(string where, object parameters);
    Task<PaginatedResult<T>> GetPaginatedAsync<T>(PaginationParams pagination);
    // + 5 more query operations...
}

// Write operations
public interface ICommandExecutor
{
    Task<int> InsertAsync<T>(T entity);
    Task<bool> UpdateAsync<T>(T entity);
    Task<bool> DeleteAsync<T>(int id);
    // + 3 more command operations...
}

// Transaction management for multi-step operations
public interface ITransactionManager
{
    Task<T> WithTransactionAsync<T>(Func<Task<T>> operation);
}
```
> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/API/Database)

### Result Pattern for Error Handling

Business logic failures return `Result<T>` instead of throwing exceptions. This makes error handling explicit and embeds HTTP status codes in the domain layer. Centralized `ErrorMessages` classes ensure consistent, DRY error responses:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public HttpStatusCode StatusCode { get; }

    public static Result<T> Success(T data) => new(true, data, null, HttpStatusCode.OK);
    public static Result<T> Failure(ErrorMessage error) => new(false, default, error.Message, error.StatusCode);
}

// Usage with centralized error messages
var product = await _queryExecutor.GetByIdAsync<Product>(id);
if (product == null)
    return Result<ProductDetailDto>.Failure(ErrorMessages.Product.NotFound(id));
```

### SQL Injection Prevention with TrustedSqlExpression

Dynamic SQL fragments like ORDER BY clauses can't be parameterized, creating SQL injection risk when user input influences sorting. `TrustedSqlExpression` is a sealed wrapper that forces developers to explicitly "trust" any SQL expression:

```csharp
public sealed class TrustedSqlExpression
{
    private readonly string _expression;

    public TrustedSqlExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("SQL expression cannot be empty", nameof(expression));
        _expression = expression;
    }

    public string ToSql() => _expression;
}

// Usage - only trusted, hardcoded expressions are wrapped:
var customOrderBy = !string.IsNullOrWhiteSpace(filterParams.Search)
    ? new TrustedSqlExpression("Relevance ASC, isDemoProduct DESC, p.productId")
    : null;
```

The type system prevents unsafe SQL construction - you can't accidentally pass user input directly to non-parameterizable SQL fragments.

### Resilience & Rate Limiting

**Polly HTTP Resilience** with three-layer policy composition:

```text
Order matters: Circuit Breaker (outer) → Retry (middle) → Timeout (inner)

1. Circuit Breaker - Opens after 5 failures, stays open 30 seconds
2. Retry - 3 attempts with exponential backoff (2s, 4s, 8s)
3. Timeout - 10 seconds per request
```

**Multi-Tier Rate Limiting:**

| Policy | Limit | Purpose |
|--------|-------|---------|
| Auth endpoints | 5 req/min | Brute force protection |
| Authenticated users | Token bucket (100 tokens, 50/min refill) | Fair usage with burst allowance |
| Anonymous users | 30 req/min sliding window | Public browsing limits |
| Expensive operations | 10 req/min | Protect image uploads |

### Authentication & Authorization

JWT-based authentication with a flexible role-based access control (RBAC) system:

- **Admin role** - Full access to all products (including other users' products)
- **Product Writer / Image Manager** - Can only manage non-demo products
- **Demo products** - Flagged separately, visible to all but only editable by admins
- **Public "Try" mode** - Unauthenticated users can explore the admin UI without creating an account

This enables the portfolio demo use case: visitors can see the full admin experience, registered users can create their own products, and demo data stays protected.

### Data Integrity

**Optimistic Concurrency** prevents lost updates when two users edit the same record:

```csharp
// Before any update, verify the record hasn't changed
public async Task UpdateAsync<T>(T obj, DateTime? expectedUpdatedAt, CancellationToken ct)
{
    await VerifyConcurrencyAsync<T>(id, expectedUpdatedAt, ct); // Throws if mismatch
    // ... proceed with update
}

// VerifyConcurrencyAsync compares timestamps (truncated to ms for JSON precision)
if (currentUpdatedAt != expectedUpdatedAt)
    throw new ConcurrencyException("Record was modified by another user"); // 409 Conflict
```
> [View full source](https://github.com/csmith468/DigitalStorefront/blob/main/server/API/Database/DataContextDapper.cs)

The frontend receives `UpdatedAt` with each record and sends it back on update. If it doesn't match, the API returns 409 Conflict and the user is prompted to refresh.

**Idempotency Keys** prevent duplicate processing on retries:

```csharp
[Authorize(Policy = "CanManageProducts")]
[Idempotent] // Custom action filter - requires Idempotency-Key header
[HttpPost]
public async Task<ActionResult<ProductDetailDto>> CreateProduct(ProductFormDto dto)
{
    // If Idempotency-Key header was seen before, returns stored result
    // Otherwise processes normally and stores result for future duplicates
}
```
> [View full source](https://github.com/csmith468/DigitalStorefront/blob/main/server/API/Services/IdempotentAttribute.cs)

The frontend axios interceptor auto-generates a UUID for each mutation and sends it via `Idempotency-Key` header. On network retry or accidental double-submit, the server returns the original response instead of creating duplicates. Keys expire after 24 hours and are cleaned up by a background service.

### Observability

- **Correlation IDs** - Every request gets a unique ID that flows through all logs
- **Serilog** - Structured logging with automatic enrichment
- **Application Insights** - Production monitoring and telemetry
- **Health Checks** - SQL Server probes with ready/alive endpoints

---

## Frontend Architecture

### React Query for Server State

All server data flows through React Query with custom hooks:

```typescript
export const useProducts = (filters: ProductFilters) => {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: ({ signal }) => getProducts(filters, signal),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
};

export const useCreateProduct = () => {
  return useMutationWithToast({
    mutationFn: (product: ProductFormRequest) => createProduct(product),
    onSuccess: (data, _, queryClient) => {
      queryClient.setQueryData(['product', data.productId], data);
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
    successMessage: SuccessMessages.Product.created, // centralized success/error messages
    errorMessage: ErrorMessages.Product.createFailed,
  })
}
```

### Custom Primitives Library

13 reusable UI components with consistent APIs and ARIA accessibility:

**Form Primitives:**
- `FormInput`, `FormSelect`, `FormTextArea`, `FormCheckbox`
- `FormChipInput` - Autocomplete with keyboard navigation (arrow keys, Enter, Escape, Backspace), fuzzy search, and full ARIA support
- `FormShell` - Generic form wrapper with validation, dirty state tracking, and unsaved changes warnings

**Layout Primitives:**
- `Modal`, `ConfirmModal`, `Tabs`, `PageHeader`, `PaginationWrapper`

All primitives share a consistent API pattern:

```typescript
interface FormPrimitiveProps<T> {
  id: string;
  label: string;
  value: T;
  onChange: (field: string, value: T) => void;
  error?: string;
  disabled?: boolean;
  required?: boolean;
}
```
> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/client/src/components/primitives)

### useMutationWithToast

Custom hook that wraps React Query mutations with automatic toast notifications, reducing boilerplate across all mutations. Options include:

```typescript
interface MutationWithToastOptions<TData, TVariables> {
  mutationFn: (variables: TVariables) => Promise<TData>;
  successMessage: string;
  errorMessage?: string;
  onSuccess?: (data: TData, variables: TVariables, queryClient: QueryClient) => void;
}
```
> [View full source](https://github.com/csmith468/DigitalStorefront/blob/main/client/src/hooks/utilities/useMutationWithToast.ts)

This pattern eliminated ~100 lines of duplicate toast handling code across the app.

### Error Boundaries

Multi-layer error boundary strategy for graceful degradation:

```tsx
// Section-level boundaries prevent one component from crashing the page
<SectionErrorBoundary sectionName="Product Form">
  <ProductFormPageShared mode="edit" />
</SectionErrorBoundary>

<SectionErrorBoundary sectionName="Product Images">
  <ProductImageManager productId={productId} />
</SectionErrorBoundary>
```

---

## Testing Strategy

This project has **unit, integration, and end-to-end tests** across both frontend and backend, with **80%+ test coverage** on critical paths.

### Backend

| Type | Tool | Description |
|------|------|-------------|
| Unit | xUnit + Moq | Business logic with mocked dependencies |
| Integration | Testcontainers | Real SQL Server 2022 in Docker containers |

**Testcontainers** spins up actual SQL Server instances, catching SQL syntax and behavior differences that in-memory databases miss:

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await RunMigrations(_container.GetConnectionString());
    }
}
```
> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/API.Tests)

### DatabaseManagement CLI

A dedicated console app handles database lifecycle with three modes:

```bash
# Production: Run pending migrations only
dotnet run -- --migrate

# Development: Clear all data but keep schema
dotnet run -- --clean

# Development/Testing: Drop everything and rebuild from scratch
dotnet run -- --reset
```
> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/DatabaseManagement)

**IUserInteraction Abstraction:**

The CLI uses an `IUserInteraction` interface to handle user prompts, enabling the same migration code to run in different contexts:

- **InteractiveUserInteraction** - Prompts for admin credentials via console (local development)
- **AutoUserInteraction** - Uses environment variables or defaults (CI/CD pipelines and Testcontainers)

This allows first-run setup to prompt for admin credentials locally (no hardcoded passwords), while CI/CD and test environments run fully automated.

### Frontend

| Type | Tool | Description |
|------|------|-------------|
| Unit | Vitest + Testing Library | 16 test files covering components, hooks, services, and contexts |
| E2E | Playwright | 3 test suites for catalog browsing, public try flow, and full auth + CRUD |

**Unit tests cover:**
- Components: `ProductForm`, `AdminProductList`, `LoginForm`, `RegisterForm`, `ProtectedRoute`
- Primitives: `FormChipInput`, `FormShell`
- Hooks: `useMutationWithToast`, `usePagination`, `useUnsavedChanges`
- Services: `apiClient`, `auth`, `metadata`, `products`
- Contexts: `UserContext`

**E2E tests cover:**
- Catalog browsing and navigation
- Public "Try" mode flow
- Full registration → login → create product → manage images flow

---

## Project Structure

```text
digital-storefront/
├── client/                      # React frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── admin/           # Admin console components
│   │   │   ├── primitives/      # Reusable UI primitives
│   │   │   └── product/         # Product display components
│   │   ├── contexts/            # React Context (auth)
│   │   ├── hooks/               # Custom React hooks
│   │   ├── pages/               # Page components
│   │   ├── services/            # API client layer
│   │   └── types/               # TypeScript definitions
│   └── e2e/                     # Playwright tests
│
├── server/
│   ├── API/                     # .NET Web API
│   │   ├── Controllers/         # HTTP endpoints
│   │   ├── Services/            # Business logic
│   │   ├── Database/            # Dapper data access layer
│   │   ├── Models/              # DTOs and entities
│   │   ├── Middleware/          # Correlation IDs, exception handling
│   │   ├── Extensions/          # DI configuration
│   │   ├── Infrastructure/      # Startup orchestration, background jobs, contexts
│   │   └── Validators/          # FluentValidation
│   ├── API.Tests/               # xUnit + Testcontainers
│   └── DatabaseManagement/      # DbUp migration CLI
│
├── docker/                      # SQL Server container config
└── .github/workflows/           # CI/CD pipelines
```

---

## Local Development

**Prerequisites:** Node.js 20+, .NET 8 SDK, Docker Desktop

```bash
# Start SQL Server container
cd docker
cp .env.example .env       # Edit .env with your credentials
docker-compose up -d

# Configure API
cd server/API
cp appsettings.example.json appsettings.Development.json   # Edit with your credentials

# Run migrations
cd server/DatabaseManagement && dotnet run -- --migrate

# Start API (http://localhost:5000)
cd server/API && dotnet run

# Start frontend (http://localhost:5173)
cd client && npm install && npm run dev
```

---

## Deployment

Push to `main` triggers GitHub Actions, which runs tests, builds, and deploys to Azure (App Service for backend, Static Web Apps for frontend).

**Before pushing, run locally:**

```bash
# Backend
cd server/DatabaseManagement && dotnet run -- --migrate
cd server/API && dotnet build
cd server/API.Tests && dotnet test

# Frontend
cd client && npm run build && npm test

# E2E (requires API running)
cd client && npx playwright test
```

---

## License

MIT License - see [LICENSE](LICENSE) for details.

---

**Built by Chapin Smith** | [Live Demo](https://digitalstorefront.dev) | [GitHub](https://github.com/csmith468/DigitalStorefront)
