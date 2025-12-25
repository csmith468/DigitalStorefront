# Digital Storefront

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.9.3-3178C6?logo=typescript)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?logo=microsoftazure)
![License](https://img.shields.io/badge/License-MIT-green)

A production-ready admin console for e-commerce product management, featuring complex form workflows, multi-image handling, RBAC, and Stripe checkout.

**Live Demo:** [digitalstorefront.dev](https://digitalstorefront.dev)

> **Try it now:** Click "Admin" then "Try It" to explore the full product management workflow without creating an account.

---

## Key Features

| Category | Highlights |
|----------|------------|
| **Backend** | Custom Dapper ORM, Result pattern, Stripe payments, SignalR real-time, Polly resilience, idempotency keys |
| **Frontend** | Custom component library (14 primitives), React Query, multi-layer error boundaries |
| **Security** | JWT + RBAC, SQL injection prevention, optimistic concurrency, multi-tier rate limiting |
| **Testing** | Testcontainers (real SQL Server), Vitest, Playwright E2E with Stripe |
| **DevOps** | GitHub Actions CI/CD, Azure (App Service, Static Web Apps, SQL, Blob, Key Vault) |

---

## Overview

Digital Storefront is a cloud-native application with a React + TypeScript frontend and .NET 8 API backend, deployed to Azure with a full CI/CD pipeline.

### Why I Built This

I logged back into a childhood online game and found their website horribly dated, which made me want to rebuild it. I started planning a storefront, but realized a shopping cart wouldn't showcase much. So I pivoted to the admin console to build complex form workflows, image management, and the production patterns that don't show up in typical portfolio projects.

**What it does:**

- Admin console for product CRUD with multi-image management
- Public "Try" mode for testing the admin form without authentication
- Stripe checkout with webhook signature verification and order tracking
- Real-time viewer count on product pages via SignalR
- Browse products by category and subcategory with tag-based search

**Infrastructure:**

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
└─────────────────────────────────────────────────────────────────┘
```

**Code architecture:**

```text
┌────────────────────────────────────────────────────────────────┐
│  FRONTEND                                                      │
│  Components → React Query Hooks → Services → Axios             │
└────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────┐
│  API GATEWAY                                                   │
│  Rate Limiting → Correlation ID → Exception Handling → Auth   │
└────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────┐
│  CONTROLLERS                                                   │
│  [Authorize] [Idempotent] [RateLimit] → Thin routing layer    │
└────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────┐
│  SERVICES                                                      │
│  Business logic returning Result<T> for explicit error handling│
└────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────┐
│  DATABASE LAYER                                                │
│  IQueryExecutor │ ICommandExecutor │ ITransactionManager       │
└────────────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technologies |
|-------|--------------|
| Frontend | React 19, TypeScript, React Query, React Router v7, Tailwind CSS, Vite |
| Backend | .NET 8, Dapper (custom ORM abstraction), FluentValidation, Polly, SignalR, Serilog |
| Payments | Stripe (PaymentIntents, Webhooks), SendGrid (order confirmations) |
| Database | SQL Server 2022, DbUp migrations |
| Testing | xUnit, Testcontainers, Vitest, Playwright |
| Cloud | Azure (App Service, Static Web Apps, SQL Database, Blob Storage, Key Vault, Application Insights) |
| DevOps | GitHub Actions, Docker |

---

## Backend Architecture

### Custom Dapper ORM Abstraction

Built a lightweight ORM layer on top of Dapper with attribute-based mapping, eliminating Entity Framework overhead while maintaining type safety:

```csharp
[DbTable("dbo.product")]
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
    Task<T?> GetByIdAsync<T>(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetByFieldAsync<T>(string fieldName, object value, CancellationToken ct = default);
    Task<(IEnumerable<T> items, int totalCount)> GetPaginatedWithSqlAsync<T>(...);
    // + 9 more query operations
}

// Write operations - includes optimistic concurrency
public interface ICommandExecutor
{
    Task<int> InsertAsync<T>(T entity, CancellationToken ct = default);
    Task UpdateAsync<T>(T entity, DateTime? expectedUpdatedAt, CancellationToken ct = default);
    Task DeleteByIdAsync<T>(int id, CancellationToken ct = default);
    // + 6 more write operations
}

// Transaction management for multi-step operations
public interface ITransactionManager
{
    Task<T> WithTransactionAsync<T>(Func<Task<T>> operation, CancellationToken ct = default);
}
```

> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/API/Database)

### Result Pattern for Error Handling

Business logic failures return `Result<T>` instead of throwing exceptions. This makes error handling explicit and embeds HTTP status codes in the domain layer:

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

// Centralized error messages ensure consistent responses
var product = await _queryExecutor.GetByIdAsync<Product>(id);
if (product == null)
    return Result<ProductDetailDto>.Failure(ErrorMessages.Product.NotFound(id));
```

### Stripe Payment Integration

Full payment flow with webhook handling and signature verification:

```csharp
// CheckoutService orchestrates order creation + Stripe PaymentIntent
public async Task<Result<PaymentIntentResponse>> ExecuteCheckoutWorkflowAsync(CreatePaymentIntentRequest request, CancellationToken ct)
{
    // Step 1: Create order in transaction
    var orderId = await _transactionManager.WithTransactionAsync(async () =>
    {
        var id = await _commandExecutor.InsertAsync(new Order { ... }, ct);
        await _commandExecutor.InsertAsync(new OrderItem { OrderId = id, ... }, ct);
        return id;
    }, ct);

    // Step 2: Create Stripe PaymentIntent with order metadata
    var paymentIntent = await new PaymentIntentService().CreateAsync(new PaymentIntentCreateOptions
    {
        Amount = totalCents,
        Currency = "usd",
        Metadata = new Dictionary<string, string> { { "order_id", orderId.ToString() } }
    });

    return Result<PaymentIntentResponse>.Success(new PaymentIntentResponse
    {
        ClientSecret = paymentIntent.ClientSecret,
        OrderId = orderId
    });
}

// WebhooksController validates Stripe signatures before processing
var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeOptions.Value.WebhookSecret);
```

> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/API/Services/Orders)

### SignalR Real-Time Viewer Tracking

Live viewer count on product pages with proper state management:

```csharp
// ViewerTrackingService manages state with thread-safe collections
public class ViewerTrackingService : IViewerTrackingService
{
    private readonly ConcurrentDictionary<string, int> _viewerCounts = new();
    private readonly ConcurrentDictionary<string, string> _connectionToProduct = new();

    public ViewerJoinResult TrackViewer(string connectionId, string productSlug)
    {
        var previousResult = UntrackViewer(connectionId);  // Handle user switching products
        _connectionToProduct[connectionId] = productSlug;
        var count = _viewerCounts.AddOrUpdate(productSlug, 1, (_, c) => c + 1);
        return new ViewerJoinResult(productSlug, count, previousResult);
    }
}

// Hub is thin orchestration layer
public class ProductViewerHub : Hub
{
    public async Task JoinProductAsync(string productSlug)
    {
        var result = _viewerTrackingService.TrackViewer(Context.ConnectionId, productSlug);
        await Groups.AddToGroupAsync(Context.ConnectionId, productSlug);
        await Clients.Group(productSlug).SendAsync("ViewerCountUpdated", result.ViewerCount);
    }
}
```

> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/API/Infrastructure/Viewers)

### SQL Injection Prevention

Dynamic SQL fragments like ORDER BY can't be parameterized. `TrustedSqlExpression` forces explicit trust for any dynamic SQL:

```csharp
public sealed class TrustedSqlExpression
{
    private readonly string _expression;
    public TrustedSqlExpression(string expression) => _expression = expression;
    public string ToSql() => _expression;
}

// Only hardcoded expressions can be wrapped - user input can't reach this path
var customOrderBy = new TrustedSqlExpression("Relevance ASC, isDemoProduct DESC, p.productId");
```

### Startup Orchestration

Prevents race conditions between hosted services on cold start:

```csharp
public class StartupOrchestrator : IHostedService
{
    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<Exception>()
        .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    public async Task StartAsync(CancellationToken ct)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await SeedRolesAsync(ct);    // Ensure roles exist
            await WarmCacheAsync(ct);    // Pre-load static data
        });
    }
}

// Background cleanup waits for startup to complete
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var lifetime = _serviceProvider.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() => startedTcs.SetResult());
    await startedTcs.Task;  // Wait for orchestrator to finish
    // ... then start cleanup loop
}
```

> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/API/Infrastructure/Startup)

### Resilience & Rate Limiting

**Polly HTTP Resilience** with three-layer policy composition:

- Circuit Breaker (outer) - Opens after 5 failures, stays open 30 seconds
- Retry (middle) - 3 attempts with exponential backoff (2s, 4s, 8s)
- Timeout (inner) - 10 seconds per request

**Multi-Tier Rate Limiting** (configurable via appsettings):

| Policy | Algorithm | Purpose |
|--------|-----------|---------|
| Auth endpoints | Fixed window | Brute force protection |
| Authenticated | Token bucket (burst + sustained) | Fair usage with burst allowance |
| Anonymous | Sliding window | Public browsing limits |
| Expensive | Fixed window | Protect image uploads, payments |

### Data Integrity

**Optimistic Concurrency** - `UpdatedAt` timestamp comparison prevents lost updates:

```csharp
public async Task UpdateAsync<T>(T obj, DateTime? expectedUpdatedAt, CancellationToken ct)
{
    await VerifyConcurrencyAsync<T>(id, expectedUpdatedAt, ct); // Throws ConcurrencyException if mismatch
    // ... proceed with update
}
```

**Idempotency Keys** - SHA256 hashes request body to prevent duplicates and detect misuse:

```csharp
[Idempotent] // Custom action filter
[HttpPost]
public async Task<ActionResult<ProductDetailDto>> CreateProduct(ProductFormDto dto)
{
    // If Idempotency-Key seen with same request hash, returns cached response
    // If seen with different hash, returns 409 Conflict (fraud prevention)
}
```

The frontend axios interceptor auto-generates UUIDs for mutations. Keys expire after 24 hours.

### Observability

- **Correlation IDs** - Every request gets a unique ID that flows through all logs
- **Serilog** - Structured logging with automatic enrichment (machine, environment, user agent)
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
    successMessage: SuccessMessages.Product.created,
    errorMessage: ErrorMessages.Product.createFailed,
  })
}
```

### SignalR Hook with Reconnection

```typescript
export function useProductViewers(productSlug: string | undefined) {
  const [viewerCount, setViewerCount] = useState<number | null>(null);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/product-viewers`)
      .withAutomaticReconnect()
      .build();

    connection.on('ViewerCountUpdated', setViewerCount);
    connection.onreconnected(() => connection.invoke('JoinProductAsync', productSlug));
    connection.start().then(() => connection.invoke('JoinProductAsync', productSlug));

    return () => { connection.stop(); };
  }, [productSlug]);

  return { viewerCount };
}
```

### Custom Primitives Library

15 reusable UI components with consistent APIs and ARIA accessibility:

**Form Primitives:**

- `FormInput`, `FormSelect`, `FormTextArea`, `FormCheckbox`, `FormLabel`
- `FormChipInput` - Autocomplete with keyboard navigation and full ARIA support
- `FormShell` - Generic form wrapper with validation, dirty state tracking, and unsaved changes warnings

**Layout Primitives:**

- `Modal`, `ConfirmModal`,`Tabs`, `TabNav`, `PageHeader`, `PaginationWrapper`, `LoadingScreen`, `OverlappingLabelBox`

> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/client/src/components/primitives)

### useMutationWithToast

Custom hook that wraps React Query mutations with automatic toast notifications:

```typescript
interface MutationWithToastOptions<TData, TVariables> {
  mutationFn: (variables: TVariables) => Promise<TData>;
  successMessage: string;
  errorMessage?: string;
  onSuccess?: (data: TData, variables: TVariables, queryClient: QueryClient) => void;
}
```

This pattern eliminated ~100 lines of duplicate toast handling code across the app.

> [View full source](https://github.com/csmith468/DigitalStorefront/blob/main/client/src/hooks/utilities/useMutationWithToast.ts)

### Error Boundaries

Multi-layer error boundary strategy for graceful degradation:

```tsx
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

A dedicated console app handles database lifecycle:

```bash
# Production: Run pending migrations only
dotnet run -- --migrate

# Development/Testing: Drop everything and rebuild from scratch
dotnet run -- --reset
```

**IUserInteraction Abstraction** enables the same migration code to run in different contexts:

- **ConsoleUserInteraction** - Prompts for admin credentials via console (local development)
- **AutoUserInteraction** - Uses environment variables (CI/CD pipelines and Testcontainers)

> [View full source](https://github.com/csmith468/DigitalStorefront/tree/main/server/DatabaseManagement)

### Frontend

| Type | Tool | Description |
|------|------|-------------|
| Unit | Vitest + Testing Library | Components, hooks, services, and contexts |
| E2E | Playwright | 4 test suites covering full user journeys |

**E2E tests cover:**

- Catalog browsing and navigation
- Public "Try" mode flow
- Full registration → login → create product → manage images flow
- Complete Stripe checkout with success/failure test cards

### E2E in CI/CD

E2E tests run in GitHub Actions against an isolated SQL Server container:

```yaml
# Each CI run gets a fresh database
- Start SQL Server container (Docker Compose with healthcheck)
- Create database + run migrations
- Start API server
- Run Playwright tests
- Cleanup container
```

This ensures complete isolation between runs - no shared state, no flaky tests from data conflicts. Branch protection requires E2E to pass before merging.

---

## Project Structure

```text
digital-storefront/
├── client/                      # React frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── admin/           # Admin console components
│   │   │   ├── checkout/        # Payment modal, Stripe integration
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
│   │   ├── Infrastructure/      # Startup orchestration, background jobs
│   │   ├── Hubs/                # SignalR hubs
│   │   └── Filters/             # Idempotency, authorization filters
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
