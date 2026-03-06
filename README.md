# Deep

A production-grade **Modular Monolith** built with **.NET 10**, implementing **Vertical Slice Architecture (VSA)**, **Domain-Driven Design (DDD)**, and the **Saga Pattern** for distributed transactions.

## 🏗️ Architecture

| Pattern | Description |
|---------|-------------|
| **Modular Monolith** | Independent modules with clear boundaries, single deployment |
| **Vertical Slice Architecture** | Features organized by use case in single files |
| **Domain-Driven Design** | Rich domain models with aggregates and value objects |
| **Saga Pattern** | Orchestrated distributed transactions via MassTransit |
| **Outbox/Inbox Pattern** | Reliable messaging with exactly-once delivery |
| **Event-Driven** | Domain events and integration events for module communication |

## 🛠️ Tech Stack

| Category | Technology |
|----------|------------|
| **Runtime** | .NET 10, C# 14 |
| **API** | ASP.NET Core Minimal APIs |
| **Database** | PostgreSQL, MongoDB |
| **Messaging** | RabbitMQ, MassTransit |
| **Caching** | Redis (Saga state) |
| **Jobs** | Hangfire |
| **Cloud Native** | .NET Aspire |
| **Data Access** | EF Core, Dapper |
| **Auth** | JWT Bearer |
| **Testing** | xUnit, FluentAssertions, Testcontainers |

## 📁 Project Structure

```
Deep/
├── src/
│   ├── api/Deep.Api/              # Composition root, endpoints
│   ├── aspire/                    # .NET Aspire orchestration
│   ├── common/                    # Shared domain & infrastructure
│   └── modules/
│       ├── Accounts/              # Authentication & authorization
│       ├── Programs/              # Program management & inventory
│       └── Transactions/          # Orders, payments, sagas
└── tests/
    ├── unit/                      # Domain tests
    ├── integration/               # Application tests
    └── architecture/              # Dependency rule tests
```

### Module Structure

Each module follows the same layout:

```
Module/
├── Deep.{Module}.Domain/              # Aggregates, entities, domain events
├── Deep.{Module}.Application/         # Features (VSA), handlers, data access
└── Deep.{Module}.IntegrationEvents/   # Cross-module event contracts
```

## 📦 Vertical Slice Architecture

Each feature is self-contained in a single file:

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

### Cancel Program Saga

```
Program Cancelled → Refund Transactions → Restore Stock → Complete
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
| `coordinator2@deep.local` | `P@ssword123!` | Coordinator |
| `coordinator3@deep.local` | `P@ssword123!` | Coordinator |

## 📋 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/accounts/register` | Register account |
| `POST` | `/accounts/login` | Get JWT token |
| `POST` | `/programs` | Create program |
| `PUT` | `/programs/{id}` | Update program |
| `GET` | `/programs` | List programs |
| `POST` | `/programs/{id}/cancel` | Cancel program (triggers saga) |
| `POST` | `/transactions` | Create transaction (triggers saga) |

## 📊 Dashboards

| Dashboard | URL |
|-----------|-----|
| Swagger | `/swagger` |
| Hangfire | `/hangfire` |
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