# Deep

**Deep** is a data engagement and extraction platform where businesses create programs for customers to purchase products and generate actionable insights.

Built as a **Modular Monolith** with **.NET 10**, it uses **VSA**, **DDD**, and the **Saga Pattern** to enable scalable, event-driven workflows with a clear path to microservices.

---

## 🏗️ Architecture

| Pattern / Concept | Description |
|-------------------|-------------|
| **Modular Monolith** | Single deployable app composed of independent, well-defined modules |
| **Vertical Slice Architecture** | Organizes code by feature/use case instead of technical layers |
| **Domain-Driven Design** | Encapsulates business logic using aggregates, entities, and value objects |
| **Saga Pattern** | Coordinates multi-step workflows with eventual consistency |
| **Outbox/Inbox Pattern** | Ensures reliable, idempotent message delivery and processing |
| **Event-Driven Communication** | Modules communicate via events instead of direct dependencies |
| **Permission-Based Authorization** | Fine-grained access control using permissions over roles |
| **Microservice-Friendly Boundaries** | Modules are structured for easy extraction into services |

---

## 🛠️ Tech Stack

| Category | Technology |
|----------|------------|
| **Runtime** | .NET 10, C# 14 |
| **API** | ASP.NET Core Minimal APIs |
| **Database** | PostgreSQL, MongoDB |
| **Messaging** | RabbitMQ, MassTransit |
| **Caching / State** | Redis |
| **Background Processing** | .NET Hosted Background Workers |
| **Cloud Native** | .NET Aspire |
| **Data Access** | EF Core, Dapper |
| **Auth** | JWT Bearer + Permission-Based Authorization |
| **Testing** | xUnit, FluentAssertions, Testcontainers |

---

## 📁 Project Structure

```text
Deep/
├── src/
│   ├── api/Deep.Api/              # Composition root, endpoints, app configuration
│   ├── aspire/                    # .NET Aspire orchestration
│   ├── common/                    # Shared domain, infrastructure, building blocks
│   └── modules/
│       ├── Accounts/              # Authentication & authorization
│       ├── Programs/              # Program management & inventory
│       └── Transactions/          # Orders, payments, sagas
└── tests/
    ├── unit/                      # Domain tests
    ├── integration/               # Application/infrastructure tests
    └── architecture/              # Dependency rule tests
```
## Module Structure

Each module follows the same layout:

```text
Module/
├── Deep.{Module}.Domain/              # Aggregates, entities, domain events
├── Deep.{Module}.Application/         # Features (VSA), handlers, data access
└── Deep.{Module}.IntegrationEvents/   # Cross-module event contracts
```
---
## 📦 Vertical Slice Architecture

Each feature is self-contained in a single file (and can be split into multiple files within a single folder if needed), enabling fast development, easier maintenance, and clear feature isolation.

```csharp
public static class CreateProgram
{
    public sealed record Command(...);
    public sealed record Response(Guid Id);
    public sealed class Handler : IRequestHandler<Command, Response> { ... }
    public sealed class Endpoint : IEndpoint { ... }
}
```

## 🔄 Saga Workflows

### Purchase Saga

```
Transaction Created → Reserve Stock → Process Payment → Complete
                           ↓                 ↓
                     Stock Failed      Payment Failed
                           ↓                 ↓
                      Fail Txn         Release Stock → Fail Txn
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run with Aspire

```bash
dotnet workload install aspire
cd src/aspire/Deep.AppHost
dotnet run
```

This starts PostgreSQL, MongoDB, RabbitMQ, Redis, and the API.

### Development Credentials

| Email | Password | Role |
|-------|----------|------|
| `admin@deep.local` | `P@ssword123!` | ItAdmin |
| `manager@deep.local` | `P@ssword123!` | Manager |
| `owner@deep.local` | `P@ssword123!` | ProgramOwner |
| `ba@deep.local` | `P@ssword123!` | BrandAmbassador |
| `coordinator1@deep.local` | `P@ssword123!` | Coordinator |


## 📊 Dashboards

| Dashboard | URL |
|-----------|-----|
| Aspire | `localhost:17169` |
| Swagger | `/swagger` |
| PgAdmin | `localhost:8001` |
| Mongo Express | `localhost:8002` |
| RabbitMQ | `localhost:8003` |

## 🧪 Testing Strategy
| Layer | Approach |
|-------|----------|
| **Domain** | Unit tests for aggregates, entities, value objects |
| **Application** | Integration tests with Testcontainers |
| **Architecture** | ArchUnit-style tests for dependency rules |
```bash
dotnet test                                    # All tests
dotnet test --filter "FullyQualifiedName~Unit"        # Unit only
dotnet test --filter "FullyQualifiedName~Integration" # Integration only
```

## 📝 License

MIT
