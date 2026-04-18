# 📋 E-GREETINGS — MICROSERVICE ARCHITECTURE RULES

> Phần bổ sung cho `project-rules.md`. Áp dụng khi chuyển đổi hoặc mở rộng dự án theo kiến trúc Microservice.
> Đọc kỹ **toàn bộ** tài liệu này trước khi bắt đầu tách service mới.

---

## 12. TỔNG QUAN KIẾN TRÚC MICROSERVICE

### Nguyên tắc cốt lõi

- **Database per Service** — mỗi service sở hữu database riêng, không service nào được truy cập DB của service khác
- **Single Responsibility** — mỗi service giải quyết đúng 1 bounded context nghiệp vụ
- **Loose Coupling** — service giao tiếp qua API Gateway (sync) hoặc Message Bus (async), không gọi trực tiếp nhau qua DbContext hay shared library
- **High Cohesion** — code liên quan đến cùng một nghiệp vụ phải nằm trong cùng service
- **Independent Deployability** — mỗi service phải build, test, và deploy độc lập

### Sơ đồ tổng thể

```
                        ┌─────────────────────────────┐
  [Angular Frontend] ───►      API Gateway             │
                        │   (YARP / Ocelot)            │
                        └──────┬──────────────┬────────┘
                               │   Route      │
              ┌────────────────┼──────────────┼──────────────────┐
              ▼                ▼              ▼                   ▼
     ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
     │ Identity     │ │ Greeting     │ │ Subscription │ │ User         │
     │ Service      │ │ Service      │ │ Service      │ │ Service      │
     │ :5001        │ │ :5002        │ │ :5003        │ │ :5004        │
     └──────┬───────┘ └──────┬───────┘ └──────┬───────┘ └──────┬───────┘
            │                │                │                │
     ┌──────┴───────┐ ┌──────┴───────┐ ┌──────┴───────┐ ┌──────┴───────┐
     │  identity_db │ │ greeting_db  │ │subscription_db│ │   user_db   │
     └──────────────┘ └──────────────┘ └──────────────┘ └─────────────┘

              ┌──────────────────────────────────────────────────┐
              │              Message Bus (RabbitMQ)              │
              │   UserRegistered | GreetingSent | PaymentConfirmed│
              └──────────────────────────────────────────────────┘
                               ▲
              ┌────────────────┼────────────────┐
              ▼                ▼                ▼
     ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
     │ Notification │ │ Admin        │ │ Feedback     │
     │ Service      │ │ Service      │ │ Service      │
     │ :5005        │ │ :5006        │ │ :5007        │
     └──────────────┘ └──────────────┘ └──────────────┘
```

---

## 13. PHÂN CHIA SERVICE

### Danh sách service và trách nhiệm

| Service | Port | Trách nhiệm | Database |
|---------|------|-------------|----------|
| **API Gateway** | 5000 | Route, Auth validation, Rate limit, SSL termination | — |
| **Identity Service** | 5001 | Đăng ký, Đăng nhập, JWT, Refresh Token, Đổi mật khẩu | `identity_db` |
| **Greeting Service** | 5002 | Template thiệp, Tùy chỉnh, Gửi thiệp, Xem thiệp | `greeting_db` |
| **Subscription Service** | 5003 | Subscribe plan, Thanh toán, Gia hạn, Recipients | `subscription_db` |
| **User Service** | 5004 | Hồ sơ cá nhân, Danh bạ, Bản nháp, Lịch sử | `user_db` |
| **Notification Service** | 5005 | Gửi email, Retry queue, Email log | `notification_db` |
| **Admin Service** | 5006 | Dashboard, Quản lý user, Audit log, Web content | `admin_db` |
| **Feedback Service** | 5007 | Gửi phản hồi, Xem phản hồi (Admin) | `feedback_db` |

### Cấu trúc thư mục solution

```
EGreetings/
├── gateway/
│   └── EGreetings.Gateway/                  ← YARP API Gateway
│       ├── appsettings.json
│       └── yarp.json                        ← Route configuration
│
├── services/
│   ├── identity/
│   │   ├── EGreetings.Identity.Domain/
│   │   ├── EGreetings.Identity.Application/
│   │   ├── EGreetings.Identity.Infrastructure/
│   │   └── EGreetings.Identity.API/
│   │
│   ├── greeting/
│   │   ├── EGreetings.Greeting.Domain/
│   │   ├── EGreetings.Greeting.Application/
│   │   ├── EGreetings.Greeting.Infrastructure/
│   │   └── EGreetings.Greeting.API/
│   │
│   ├── subscription/
│   ├── user/
│   ├── notification/
│   ├── admin/
│   └── feedback/
│
├── shared/
│   ├── EGreetings.Shared.Contracts/         ← Event/Message contracts (DTOs dùng chung)
│   ├── EGreetings.Shared.Infrastructure/    ← Middleware, Health check, Logging dùng chung
│   └── EGreetings.Shared.Domain/            ← BaseEntity, Result<T>, ValueObjects
│
├── frontend/
│   └── EGreetings.Angular/
│
├── docker-compose.yml
├── docker-compose.override.yml
└── EGreetings.sln
```

### Quy tắc cấu trúc từng service

Mỗi service **bắt buộc** tuân theo Clean Architecture 4 lớp (giống rules hiện tại):

```
{Service}.Domain          → Entities, Enums, Domain Events của service đó
{Service}.Application     → Commands/Queries, DTOs, Interfaces, Validators
{Service}.Infrastructure  → EF Core, Repository, Message Bus consumer/publisher
{Service}.API             → Controllers, Middleware, DI, Health Check
```

---

## 14. API GATEWAY

### Công nghệ: YARP (Yet Another Reverse Proxy)

```csharp
// EGreetings.Gateway/Program.cs
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

### Nhiệm vụ của Gateway — KHÔNG làm gì thêm ngoài danh sách này

- Route request đến đúng service theo path prefix
- Validate JWT token (xác thực token hợp lệ — **không xử lý business logic**)
- Rate limiting theo IP và theo user
- SSL termination
- Request/Response logging
- CORS policy tập trung

### Route convention

```json
// yarp.json
{
  "/api/auth/**"          → Identity Service :5001
  "/api/greetings/**"     → Greeting Service :5002
  "/api/templates/**"     → Greeting Service :5002
  "/api/subscriptions/**" → Subscription Service :5003
  "/api/users/**"         → User Service :5004
  "/api/admin/**"         → Admin Service :5006
  "/api/feedback/**"      → Feedback Service :5007
}
```

### Quy tắc Gateway

- Gateway **không có database**, **không có business logic**
- Authorization (phân quyền chi tiết) thực hiện tại từng service, không phải Gateway
- Gateway chỉ kiểm tra token hợp lệ, **không** kiểm tra role/permission
- Timeout mặc định mỗi upstream: **30 giây**
- Nếu service down → trả về `503 Service Unavailable` với message rõ ràng

---

## 15. GIAO TIẾP GIỮA CÁC SERVICE

### Nguyên tắc chọn kiểu giao tiếp

| Trường hợp | Kiểu | Công nghệ |
|------------|------|-----------|
| Client → Service (HTTP request/response) | Synchronous | REST qua API Gateway |
| Service → Service, không cần response ngay | Asynchronous | RabbitMQ + MassTransit |
| Service → Service, cần response ngay | Synchronous | HTTP với `IHttpClientFactory` |
| Service A cần dữ liệu của Service B | Async query | Event Sourcing hoặc local cache |

### Message Bus — RabbitMQ + MassTransit

**Cài đặt trong mỗi service:**

```csharp
// Program.cs của mỗi service
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
        cfg.ConfigureEndpoints(ctx);
    });
});
```

### Event/Message Contracts — đặt trong `EGreetings.Shared.Contracts`

```csharp
// EGreetings.Shared.Contracts/Events/Identity/UserRegisteredEvent.cs
namespace EGreetings.Shared.Contracts.Events.Identity;

public record UserRegisteredEvent(
    int UserId,
    string Email,
    string FullName,
    DateTime RegisteredAt
);

// EGreetings.Shared.Contracts/Events/Greeting/GreetingSentEvent.cs
namespace EGreetings.Shared.Contracts.Events.Greeting;

public record GreetingSentEvent(
    int GreetingId,
    int? UserId,
    string RecipientEmail,
    string TemplateName,
    DateTime SentAt
);

// EGreetings.Shared.Contracts/Events/Subscription/PaymentConfirmedEvent.cs
public record PaymentConfirmedEvent(
    int PaymentId,
    int UserId,
    int SubscriptionId,
    decimal Amount,
    DateTime PaidAt
);
```

### Quy tắc Events

- Event name: **PastTense**, danh từ đứng trước — `UserRegistered`, `GreetingSent`, `PaymentConfirmed`
- Command name (message yêu cầu service khác làm gì): **ImperativeVerb** — `SendWelcomeEmail`, `ExpireSubscription`
- Không đặt logic trong event contract — chỉ là data transfer object
- Mọi event phải có **timestamp** (`OccurredAt` hoặc `*At`)
- Dùng **`record`** (immutable) cho tất cả contracts

### Publisher — trong Application layer của service

```csharp
// GreetingService: sau khi gửi thiệp thành công
public class SendGreetingCommandHandler : IRequestHandler<SendGreetingCommand, GreetingDto>
{
    private readonly IPublishEndpoint _publishEndpoint;  // MassTransit

    public async Task<GreetingDto> Handle(SendGreetingCommand request, CancellationToken ct)
    {
        // ... business logic ...

        await _publishEndpoint.Publish(new GreetingSentEvent(
            greeting.Id, greeting.UserId,
            greeting.RecipientEmail, template.Name, DateTime.UtcNow), ct);

        return dto;
    }
}
```

### Consumer — trong Infrastructure layer của service nhận

```csharp
// NotificationService: nhận event từ GreetingService
namespace EGreetings.Notification.Infrastructure.Consumers;

public class GreetingSentEventConsumer : IConsumer<GreetingSentEvent>
{
    private readonly IEmailService _emailService;

    public async Task Consume(ConsumeContext<GreetingSentEvent> context)
    {
        await _emailService.SendGreetingEmailAsync(
            context.Message.RecipientEmail,
            context.Message.TemplateName,
            context.CancellationToken);
    }
}
```

### Idempotency — bắt buộc cho mọi Consumer

- Mỗi consumer phải kiểm tra xem event đã xử lý chưa (bằng `EventId` hoặc `MessageId`)
- Lưu processed event ID vào DB hoặc Redis với TTL 24 giờ
- Nếu event đã xử lý → skip, không throw exception

```csharp
public async Task Consume(ConsumeContext<GreetingSentEvent> context)
{
    var messageId = context.MessageId?.ToString() ?? context.Message.GreetingId.ToString();

    if (await _idempotencyRepo.ExistsAsync(messageId))
        return; // đã xử lý rồi, skip

    // ... xử lý ...

    await _idempotencyRepo.MarkProcessedAsync(messageId);
}
```

---

## 16. DATABASE PER SERVICE

### Quy tắc bắt buộc

- **Mỗi service có database riêng** — tên database theo format `egreetings_{service}_db`
- **Không service nào** được có foreign key tham chiếu sang database của service khác
- Khi cần dữ liệu của service khác: **replicate qua event** hoặc gọi API
- Mỗi service có migration folder riêng của mình
- Schema changes của service không được ảnh hưởng service khác

### Convention tên database

```
egreetings_identity_db
egreetings_greeting_db
egreetings_subscription_db
egreetings_user_db
egreetings_notification_db
egreetings_admin_db
egreetings_feedback_db
```

### Shared data strategy

Khi Service A cần UserId/Email của User từ Identity Service:

```
❌ Sai: SELECT * FROM identity_db.Users WHERE Id = @id  (cross-DB query)

✅ Đúng — Option 1 (Event-driven replication):
  Identity Service publish UserRegisteredEvent
  → User Service consume và lưu local copy (chỉ UserId + Email + FullName)

✅ Đúng — Option 2 (API call khi cần real-time):
  User Service gọi GET http://identity-service/api/internal/users/{id}
  (endpoint internal, không expose qua Gateway)
```

---

## 17. DOCKER VÀ CONTAINERIZATION

### Dockerfile chuẩn cho mỗi service

```dockerfile
# Đặt tại: services/{service}/{Service}.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["services/greeting/EGreetings.Greeting.API/EGreetings.Greeting.API.csproj", "services/greeting/EGreetings.Greeting.API/"]
COPY ["services/greeting/EGreetings.Greeting.Application/EGreetings.Greeting.Application.csproj", "services/greeting/EGreetings.Greeting.Application/"]
COPY ["services/greeting/EGreetings.Greeting.Infrastructure/EGreetings.Greeting.Infrastructure.csproj", "services/greeting/EGreetings.Greeting.Infrastructure/"]
COPY ["services/greeting/EGreetings.Greeting.Domain/EGreetings.Greeting.Domain.csproj", "services/greeting/EGreetings.Greeting.Domain/"]
COPY ["shared/EGreetings.Shared.Contracts/EGreetings.Shared.Contracts.csproj", "shared/EGreetings.Shared.Contracts/"]

RUN dotnet restore "services/greeting/EGreetings.Greeting.API/EGreetings.Greeting.API.csproj"

COPY . .
WORKDIR "/src/services/greeting/EGreetings.Greeting.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGreetings.Greeting.API.dll"]
```

### docker-compose.yml

```yaml
version: '3.9'

services:
  # ── Infrastructure ──────────────────────────────────────────────
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"    # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "check_port_connectivity"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"

  # ── Services ────────────────────────────────────────────────────
  gateway:
    build:
      context: .
      dockerfile: gateway/EGreetings.Gateway/Dockerfile
    ports:
      - "5000:8080"
    depends_on:
      - identity-service
      - greeting-service

  identity-service:
    build:
      context: .
      dockerfile: services/identity/EGreetings.Identity.API/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ConnectionStrings__Default=Server=sqlserver;Database=egreetings_identity_db;...
      - RabbitMQ__Host=rabbitmq
    depends_on:
      rabbitmq:
        condition: service_healthy

  greeting-service:
    build:
      context: .
      dockerfile: services/greeting/EGreetings.Greeting.API/Dockerfile
    ports:
      - "5002:8080"
    environment:
      - ConnectionStrings__Default=Server=sqlserver;Database=egreetings_greeting_db;...
      - RabbitMQ__Host=rabbitmq

  subscription-service:
    build:
      context: .
      dockerfile: services/subscription/EGreetings.Subscription.API/Dockerfile
    ports:
      - "5003:8080"

  user-service:
    build:
      context: .
      dockerfile: services/user/EGreetings.User.API/Dockerfile
    ports:
      - "5004:8080"

  notification-service:
    build:
      context: .
      dockerfile: services/notification/EGreetings.Notification.API/Dockerfile
    ports:
      - "5005:8080"

  admin-service:
    build:
      context: .
      dockerfile: services/admin/EGreetings.Admin.API/Dockerfile
    ports:
      - "5006:8080"

  feedback-service:
    build:
      context: .
      dockerfile: services/feedback/EGreetings.Feedback.API/Dockerfile
    ports:
      - "5007:8080"
```

### docker-compose.override.yml (chỉ dùng khi dev local)

```yaml
# Override cho môi trường development
services:
  identity-service:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
    volumes:
      - ./services/identity:/app/src   # hot reload

  greeting-service:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

---

## 18. HEALTH CHECK

Mỗi service **bắt buộc** expose health check endpoint. Đặt trong `Shared.Infrastructure`:

```csharp
// EGreetings.Shared.Infrastructure/Extensions/HealthCheckExtensions.cs
public static class HealthCheckExtensions
{
    public static IServiceCollection AddServiceHealthChecks(
        this IServiceCollection services,
        IConfiguration config,
        string dbConnectionStringKey = "ConnectionStrings:Default")
    {
        services.AddHealthChecks()
            .AddSqlServer(config[dbConnectionStringKey]!, name: "database")
            .AddRabbitMQ(config["RabbitMQ:ConnectionString"]!, name: "rabbitmq")
            .AddRedis(config["Redis:ConnectionString"]!, name: "redis");

        return services;
    }
}
```

```csharp
// Program.cs của mỗi service
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false   // chỉ kiểm tra process còn sống
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

---

## 19. LOGGING VÀ DISTRIBUTED TRACING

### Correlation ID — bắt buộc

Mỗi request qua Gateway phải có `X-Correlation-ID` header. Service phải forward header này sang service khác khi gọi internal API.

```csharp
// EGreetings.Shared.Infrastructure/Middleware/CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("ServiceName", ServiceName))
        {
            await next(context);
        }
    }
}
```

### Structured Logging — format thống nhất

```csharp
// Mọi service dùng chung Serilog format sau
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "greeting-service")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(new JsonFormatter())          // JSON format cho log aggregator
    .WriteTo.Seq(builder.Configuration["Seq:Url"]) // Seq cho local dev
    .CreateLogger();
```

### OpenTelemetry — distributed tracing (production)

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter()); // Jaeger hoặc Zipkin
```

---

## 20. CONFIGURATION VÀ SECRETS

### Cấu trúc appsettings của mỗi service

```json
{
  "ServiceName": "greeting-service",
  "ConnectionStrings": {
    "Default": "see environment variable CONNECTIONSTRINGS__DEFAULT"
  },
  "Jwt": {
    "Issuer": "EGreetings",
    "Audience": "EGreetings-Users",
    "SecretKey": "see environment variable JWT__SECRETKEY"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "see environment variable"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "InternalServices": {
    "IdentityService": "http://identity-service:8080",
    "NotificationService": "http://notification-service:8080"
  }
}
```

### Quy tắc configuration

- Secret (password, key, connection string) → **Environment Variables** hoặc **Vault**, không commit vào Git
- Config không phải secret → `appsettings.json` trong repo
- Môi trường dev → `appsettings.Development.json` (gitignore nếu có secret)
- Môi trường prod → **Kubernetes Secrets** hoặc **AWS Secrets Manager**
- Không dùng chung `appsettings.json` giữa các service

---

## 21. SECURITY GIỮA CÁC SERVICE

### JWT Validation

- Gateway validate JWT (chữ ký, hết hạn) trước khi forward request
- Service nhận forward thêm `X-UserId` và `X-UserRole` header (gateway extract từ JWT)
- Service **không cần validate lại JWT** — chỉ đọc header do Gateway truyền xuống

```csharp
// Gateway extract claims và forward
context.Request.Headers["X-UserId"] = user.FindFirst("sub")?.Value;
context.Request.Headers["X-UserRole"] = user.FindFirst("role")?.Value;
```

```csharp
// Service đọc từ header thay vì parse JWT
public class CurrentUserService : ICurrentUserService
{
    public int? UserId => int.TryParse(
        _httpContextAccessor.HttpContext?.Request.Headers["X-UserId"], out var id) ? id : null;
    public string? Role =>
        _httpContextAccessor.HttpContext?.Request.Headers["X-UserRole"];
}
```

### Internal API (service-to-service)

- Endpoint nội bộ (`/internal/**`) **không được expose qua Gateway**
- Bảo vệ internal endpoint bằng **shared secret header** hoặc **mTLS**

```csharp
// Internal API authentication
[ServiceFilter(typeof(InternalApiAuthFilter))]
[Route("api/internal/users/{id}")]
public async Task<IActionResult> GetUserInternal(int id) { ... }

// InternalApiAuthFilter kiểm tra header:
// X-Internal-Secret: {giá trị từ config, không phải JWT}
```

---

## 22. QUY TẮC PHÁT TRIỂN SERVICE MỚI

Checklist bắt buộc khi tạo service mới:

```
□ Tạo đủ 4 layer: Domain / Application / Infrastructure / API
□ Thêm Dockerfile
□ Thêm vào docker-compose.yml
□ Cấu hình Health Check endpoint /health
□ Cấu hình Serilog với ServiceName property
□ Thêm CorrelationIdMiddleware
□ Thêm Global Exception Middleware
□ Cấu hình MassTransit (kể cả nếu chưa dùng — chuẩn bị sẵn)
□ Tạo database riêng, chạy initial migration
□ Thêm route vào Gateway (yarp.json)
□ Đăng ký service URL vào InternalServices config của service liên quan
□ Viết unit test cho ít nhất 1 handler
□ Thêm README.md mô tả service làm gì, port nào, event nào publish/consume
```

---

## 23. QUY TẮC SHARED LIBRARY

### `EGreetings.Shared.Contracts` — CHỈ chứa

- Event/Message record types (dùng cho RabbitMQ)
- Integration DTO (dữ liệu trao đổi giữa service qua API)
- **Không** chứa business logic, entity, service, repository

### `EGreetings.Shared.Infrastructure` — chứa

- `CorrelationIdMiddleware`
- `GlobalExceptionMiddleware`
- `HealthCheckExtensions`
- `CurrentUserService` (đọc từ header)
- Serilog configuration helper
- `InternalApiAuthFilter`

### `EGreetings.Shared.Domain` — chứa

- `BaseEntity` (Id, CreatedAt, UpdatedAt)
- `Result<T>` pattern
- Common Value Objects (Email, Money...)
- **Không** chứa entity cụ thể của từng service

### Quy tắc shared

- Không thêm business logic của một service vào shared library
- Khi cần nâng version shared contract → backward compatible (không xóa field, chỉ thêm)
- Breaking change trong contract → tạo version mới (`UserRegisteredEventV2`) và giữ V1 trong thời gian chuyển tiếp

---

## 24. CI/CD PER SERVICE

Mỗi service có pipeline riêng trong `.github/workflows/`:

```yaml
# .github/workflows/greeting-service.yml
name: Greeting Service CI/CD

on:
  push:
    paths:
      - 'services/greeting/**'
      - 'shared/**'
  pull_request:
    paths:
      - 'services/greeting/**'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet restore services/greeting/EGreetings.Greeting.API/EGreetings.Greeting.API.csproj
      - run: dotnet build services/greeting/ --no-restore
      - run: dotnet test services/greeting/ --no-build

  docker-build:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      - name: Build Docker image
        run: docker build -t egreetings/greeting-service:${{ github.sha }} -f services/greeting/EGreetings.Greeting.API/Dockerfile .
```

---

## 25. MIGRATION STRATEGY (MONOLITH → MICROSERVICE)

### Thứ tự tách service được khuyến nghị

Tách **từng service một**, không tách đồng thời nhiều service:

```
Bước 1: Tách Notification Service (ít dependency nhất)
         → Chỉ consume events, không có API public
Bước 2: Tách Identity Service (Auth độc lập)
         → Tất cả service còn lại cần nó
Bước 3: Tách Feedback Service (domain đơn giản)
Bước 4: Tách User Service (profile, contacts, drafts)
Bước 5: Tách Subscription Service
Bước 6: Tách Greeting Service (core business)
Bước 7: Tách Admin Service (aggregates data từ các service khác)
Bước 8: Add API Gateway, bỏ monolith
```

### Strangler Fig Pattern — tách dần dần

Trong giai đoạn chuyển tiếp, monolith và microservice chạy song song:

```
Client → Gateway → [Monolith :5000]     ← route mặc định
                 → [Identity :5001]     ← khi Identity Service sẵn sàng
```

- Khi service mới sẵn sàng → cập nhật Gateway route sang service mới
- Sau khi traffic chuyển hoàn toàn → xóa code tương ứng khỏi monolith
- **Không xóa monolith code trước khi service mới đã stable trên production**

---

> **Nguyên tắc vàng Microservice:** Nếu hai service cần share database để hoạt động,
> thì chúng không nên là hai service riêng biệt — hãy gộp lại thành một.
> Microservice boundary = Business boundary, không phải technical boundary.
