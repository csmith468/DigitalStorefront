# Digital Storefront

A production-ready full-stack e-commerce admin console for managing a virtual pet collectibles catalog.

**Live Demo:** [digitalstorefront.dev](https://digitalstorefront.dev)

---

## Overview

Digital Storefront is a cloud-native application with a React + TypeScript frontend and .NET 8 API backend, deployed to Azure with a full CI/CD pipeline.

**What it does:**
- Admin console for product CRUD with multi-image management
- Public "Try" mode for testing the admin form without authentication
- Browse products by category and subcategory
- Search products with tag-based filtering

**Architecture at a glance:**

```
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

### SQL Injection Prevention with TrustedOrderByExpression

Dynamic ORDER BY clauses can't be parameterized, creating SQL injection risk. `TrustedOrderByExpression` uses a whitelist approach:

```csharp
public class TrustedOrderByExpression
{
    private static readonly HashSet<string> AllowedColumns =
        new() { "Name", "Price", "CreatedAt", "UpdatedAt" };

    public static TrustedOrderByExpression? Create(string column, string direction)
    {
        if (!AllowedColumns.Contains(column)) return null;
        if (direction != "ASC" && direction != "DESC") return null;

        return new TrustedOrderByExpression($"{column} {direction}");
    }
}
```

The type system prevents unsafe ORDER BY construction - you can't accidentally pass user input directly to SQL.

### Resilience & Rate Limiting

**Polly HTTP Resilience** with three-layer policy composition:

```
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
- **Standard users** - Can only manage non-demo products
- **Demo products** - Flagged separately, visible to all but only editable by admins
- **Public "Try" mode** - Unauthenticated users can explore the admin UI without creating an account

This enables the portfolio demo use case: visitors can see the full admin experience, registered users can create their own products, and demo data stays protected.

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

## Local Development

### Prerequisites
- Node.js 20+
- .NET 8 SDK
- Docker Desktop

### Quick Start

```bash
# 1. Start SQL Server container
cd database/Docker
cp .env.example .env
docker-compose up -d

# 2. Run migrations
cd server/DatabaseManagement
dotnet run -- --migrate "Server=localhost;Database=DigitalStorefront;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"

# 3. Start API (http://localhost:5000)
cd server/API
dotnet run

# 4. Start frontend (http://localhost:5173)
cd client
npm install && npm run dev
```

---

## Project Structure

```
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
│   │   └── Validators/          # FluentValidation
│   ├── API.Tests/               # xUnit + Testcontainers
│   └── DatabaseManagement/      # DbUp migration CLI
│
├── database/Docker/             # SQL Server container config
└── .github/workflows/           # CI/CD pipelines
```

---

## Deployment

Push to `main` triggers GitHub Actions:

**Backend Pipeline** (`server/` changes):
1. Build and run tests
2. Run DbUp migrations against Azure SQL
3. Deploy to Azure App Service

**Frontend Pipeline** (`client/` changes):
1. Build with Vite
2. Deploy to Azure Static Web Apps

---

**Built by Chapin Smith** | [digitalstorefront.dev](https://digitalstorefront.dev)
