# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore dependencies
dotnet restore FlowForge.sln

# Build
dotnet build FlowForge.sln

# Run the API
dotnet run --project FlowForge.API/FlowForge.API.csproj

# Run all tests
dotnet test FlowForge.Domain.Tests/FlowForge.Domain.Tests.csproj

# Run a single test
dotnet test FlowForge.Domain.Tests/FlowForge.Domain.Tests.csproj --filter "FullyQualifiedName~TestClassName.MethodName"

# EF Core migrations
dotnet ef migrations add <MigrationName> -p FlowForge.Persistence -s FlowForge.API
dotnet ef database update -p FlowForge.Persistence -s FlowForge.API

# Start infrastructure (PostgreSQL, RabbitMQ, Redis)
docker-compose up -d
```

## Architecture

Clean Architecture with 5 layers, each as a separate .NET 8 project:

- **FlowForge.Domain** — Entities, value objects, enums, interfaces, and the `Result<T>` error pattern. No external dependencies.
- **FlowForge.Application** — CQRS handlers (MediatR), MassTransit consumers, DTOs, and abstractions (`ICurrentTenant`, `ICorrelationContext`, `IRateLimiter`, etc.)
- **FlowForge.Persistence** — EF Core + Npgsql (PostgreSQL). `FlowForgeAPIDbContext`, repository implementations, migrations.
- **FlowForge.Infrastructure** — Concrete external-service implementations: `WebhookSender` (HMAC-SHA256 signed HTTP), `RedisRateLimiter`, `ApiKeyValidationCache`, `CorrelationContext`, MassTransit/RabbitMQ wiring, Polly resilience policies.
- **FlowForge.API** — ASP.NET Core controllers, background workers, middleware, MediatR pipeline behaviors, and Serilog configuration.

## Domain Model

The core multi-tenancy unit is **Tenant** (with a Plan: Free/Starter/Pro/Enterprise and associated PlanLimits).

- `WebhookEndpoint` belongs to a Tenant and holds the target URL, `RetryPolicy`, and event type subscriptions.
- `WebhookDelivery` tracks a single delivery attempt (payload, `DeliveryStatus`, attempts, retry backoff).
- `ApiKey` / `Membership` / `User` handle authentication and authorization.

## Key Patterns

**Result<T>**: All domain and application operations return `Result<T>` or `Result` (railway-oriented programming). Controllers map these via `HandleResult()` in `BaseApiController`.

**CQRS via MediatR**: Commands and queries live under `FlowForge.Application/Features/Commands` and `FlowForge.Application/Features/Queries`. Every handler implements `IRequestHandler<TRequest, Result<T>>`.

**MediatR Pipeline Behavior**: `LoggingPipelineBehavior` in `FlowForge.API/Behaviors/` wraps all handlers with structured logging.

**Background Workers**: Two hosted services in `FlowForge.API/BackgroundServices/`:
- `DeliveryProcessorWorker` — polls and dispatches pending webhook deliveries via RabbitMQ.
- `DeliveryRecoveryWorker` — retries stuck/failed deliveries based on retry policy.

**Authentication**: Custom API key scheme (`X-Api-Key` header). `ApiKeyAuthenticationHandler` validates keys against the DB with a Redis cache layer. JWT claims carry `TenantId` and `ApiKeyId`.

**Correlation IDs**: `CorrelationIdMiddleware` sets a correlation ID per request; `CorrelationContext` (scoped service) makes it available throughout the pipeline and Serilog log context.

**Polly Resilience**: The `WebhookSender` HTTP client is configured with exponential-backoff retry (3 attempts) and circuit breaker (5 failures → 30 s break).

**Structured Logging**: Serilog with `Serilog.Formatting.Compact` and enrichers for environment, thread, and correlation ID. Development uses human-readable output with `{CorrelationId}` in the template.

## Infrastructure Dependencies

| Service | Port | Purpose |
|---|---|---|
| PostgreSQL 17 | 5432 | Primary database |
| RabbitMQ 3.13 | 5672 / 15672 (management) | Message broker |
| Redis 8 | 6379 | API key cache + rate limiting |

Credentials are in `docker-compose.yml` / `.env` and mirrored in `appsettings.json`. The `.env` file must exist before running `docker-compose up`.

## Development Status
- Domain layer: COMPLETE. Do not modify domain entities, value objects, or aggregates unless explicitly asked.
- Infrastructure, Application, API layers: Under construction.

## Coding Conventions
- Value objects use private constructors + static Create() factory returning Result<T>
- Aggregate roots enforce invariants inside the domain; never bypass via setters
- Do not use public setters on domain entities

## Rules
- Never scaffold or generate migration without being explicitly asked
- Never modify domain layer files without explicit instruction
- Do not add new NuGet packages without asking first
