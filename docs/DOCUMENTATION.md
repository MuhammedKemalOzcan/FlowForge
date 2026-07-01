# FlowForge — Technical Documentation

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture](#2-architecture)
3. [Infrastructure Dependencies](#3-infrastructure-dependencies)
4. [Domain Model](#4-domain-model)
5. [State Machines](#5-state-machines)
6. [Key Patterns](#6-key-patterns)
7. [API Reference](#7-api-reference)
8. [Background Workers](#8-background-workers)
9. [Security](#9-security)
10. [Observability](#10-observability)
11. [Development Setup](#11-development-setup)

---

## 1. Overview

FlowForge is a **multi-tenant webhook delivery platform**. It accepts events, routes them to tenant-configured HTTPS endpoints, tracks every delivery attempt, retries failures with configurable backoff policies, and streams real-time lifecycle events to connected clients via Server-Sent Events.

### Core Capabilities

| Feature | Description |
|---|---|
| Reliable delivery | RabbitMQ-backed queue + automatic retry with exponential/linear/fixed backoff |
| Real-time streaming | SSE stream of delivery lifecycle events per tenant/endpoint/delivery |
| Payload signing | Every outgoing request carries an HMAC-SHA256 signature |
| Rate limiting | Redis sliding-window rate limiter scoped per tenant plan |
| Dead-letter | Exhausted deliveries held for manual requeue |
| Demo mode | Self-destructing 24-hour tenant with auto-cleanup |
| Health checks | PostgreSQL + Redis readiness probes |

---

## 2. Architecture

FlowForge follows **Clean Architecture** with five .NET 8 projects and a strict inward dependency rule.

```
┌────────────────────────────────────────────────────────┐
│                     FlowForge.API                      │  Controllers, Workers, Middleware
│  ┌──────────────────────────────────────────────────┐  │
│  │              FlowForge.Infrastructure            │  │  Redis, RabbitMQ, HTTP, Polly
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │            FlowForge.Persistence           │  │  │  EF Core, PostgreSQL, Migrations
│  │  │  ┌──────────────────────────────────────┐  │  │  │
│  │  │  │        FlowForge.Application         │  │  │  │  CQRS, MediatR, DTOs, Abstractions
│  │  │  │  ┌────────────────────────────────┐  │  │  │  │
│  │  │  │  │       FlowForge.Domain         │  │  │  │  │  Entities, Value Objects, Enums
│  │  │  │  └────────────────────────────────┘  │  │  │  │
│  │  │  └──────────────────────────────────────┘  │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Responsibility |
|---|---|
| **Domain** | All business rules — entities, value objects, enums, repository interfaces, `Result<T>` pattern. No external dependencies. |
| **Application** | MediatR CQRS handlers, MassTransit consumers, DTOs, abstractions (`ICurrentTenant`, `ICorrelationContext`, `IRateLimiter`, etc.) |
| **Persistence** | EF Core + Npgsql, `FlowForgeAPIDbContext`, entity configurations, migrations |
| **Infrastructure** | External service implementations: `WebhookSender` (HTTP + Polly), `RedisRateLimiter`, `ApiKeyValidator` (Redis cache-first), MassTransit/RabbitMQ wiring |
| **API** | ASP.NET Core controllers, hosted background services, middleware, MediatR pipeline behaviors, Serilog configuration |

---

## 3. Infrastructure Dependencies

| Service | Port | Purpose |
|---|---|---|
| PostgreSQL 17 | 5432 | Primary database |
| RabbitMQ 3.13 | 5672 / 15672 | Message broker / management UI |
| Redis 8 | 6379 | API key cache + rate limiting |

---

## 4. Domain Model

### Aggregate Roots

**`Tenant`** is the central multi-tenancy unit. All other resources (endpoints, deliveries, API keys) are scoped to a tenant. It holds a `Plan`, computed `PlanLimits`, and a collection of `Membership` records. A newly created tenant starts on the Free plan with the creator automatically assigned the `Owner` role.

**`WebhookEndpoint`** belongs to a tenant. It stores a target HTTPS `Url`, a `RetryPolicy` snapshot, a `SigningSecret`, and a set of subscribed `EventType`s. An endpoint must always have at least one event type subscription.

**`WebhookDelivery`** tracks a single dispatch event from arrival to final outcome. It carries a copy of the `RetryPolicy` at creation time (so that subsequent policy changes on the endpoint do not affect in-flight deliveries) and accumulates `DeliveryAttempt` child records for each HTTP try.

**`ApiKey`** stores only the SHA-256 hash of the key — the plain-text value is generated once, returned to the caller, and never persisted. Each key carries a short visible prefix (first 16 characters) for identification purposes.

**`User`** is a local mirror of an identity provider record. It is kept in sync via `SyncFromIdentityProvider()` whenever the provider reports a profile change.

### Value Objects

All value objects use a private constructor + static `Create()` factory that returns `Result<T>`.

| Value Object | Notable Rules |
|---|---|
| `Url` | Must be an absolute HTTPS URL; localhost and private IP ranges are rejected |
| `EventType` | Max 100 characters; must follow `namespace.action` format (e.g. `payment.succeeded`) |
| `IdempotencyKey` | 1–255 ASCII printable characters |
| `SigningSecret` | Min 40 characters; `ToString()` always returns `***PROTECTED***`; computes HMAC-SHA256 |
| `RetryPolicy` | MaxAttempts 1–10; MaxDelay ≥ InitialDelay; Timeout > 0; three backoff strategies |
| `HashedApiKey` | Stores the SHA-256 hash; plain-text is never held after construction |

#### `RetryPolicy` — Backoff Calculation

```
Fixed        → delay = InitialDelay
Linear       → delay = InitialDelay × attemptNumber
Exponential  → delay = InitialDelay × 2^(attemptNumber - 1)
```

Calculated delay is capped at `MaxDelay`. Default policy: 5 attempts, Exponential, 1 s initial, 5 min max, 10 s timeout.

### Plan Limits

| Plan | Max Endpoints | Max Events/min | Max Members |
|---|---|---|---|
| Free | 1 | 6 | 1 |
| Starter | 5 | 100 | 3 |
| Pro | 25 | 1 000 | 10 |
| Enterprise | 50 | 10 000 | 50 |

Limits are stored as `PlanLimits` inside the `Tenant` aggregate and automatically updated on plan change.

---

## 5. State Machines

### Webhook Delivery Status

This is the most critical state machine in the system. Every transition is enforced inside the domain entity — invalid transitions throw `InvalidOperationException`.

```
                   ┌─────────────────────────────────────────────────┐
                   │                                                 │
                   ▼                                                 │
              ┌─────────┐                                            │
       ──────►│ Pending │◄──────────────────────────────────────┐   │
              └────┬────┘                                       │   │
                   │ MarkQueued()                               │   │
                   │ (only from Pending)                        │   │
                   ▼                                            │   │
              ┌─────────┐         RecoverStuckToPending()       │   │
              │ Queued  │─────────────────────────────────────► │   │
              └────┬────┘  (only from Queued or InProgress)     │   │
                   │ MarkInProgress()                           │   │
                   │ (only from Queued)                         │   │
                   ▼                                            │   │
            ┌────────────┐   RecoverStuckToPending()            │   │
            │ InProgress │──────────────────────────────────────┘   │
            └──────┬─────┘                                          │
                   │                                                 │
       ┌───────────┼───────────────────────────────┐                │
       │           │                               │                │
       │ (success) │              (failure, not    │ (failure,      │
       ▼           │               last attempt)   │  last attempt) │
  ┌─────────┐      │                               │                │
  │Succeeded│      │                               ▼                │
  └─────────┘      │                       ┌─────────────┐          │
                   │                       │ DeadLettered│          │
                   │                       └──────┬──────┘          │
                   │                              │                 │
                   │                              │ RequeueFromDeadLetter()
                   └──────────────────────────────┼─────────────────┘
                                                  │ (only from DeadLettered)
                                                  ▼
                                               Pending (NextRetryAt = now)
```

**Transition rules at a glance:**

| From | To | Trigger | Guard |
|---|---|---|---|
| `Pending` | `Queued` | `MarkQueued()` | Must be `Pending` |
| `Queued` | `InProgress` | `MarkInProgress()` | Must be `Queued` |
| `InProgress` | `Succeeded` | `RecordSuccessfulAttempt()` | Must be `InProgress` |
| `InProgress` | `Pending` | `RecordFailedAttempt()` (non-final) | Must be `InProgress`; attempt < MaxAttempts |
| `InProgress` | `DeadLettered` | `RecordFailedAttempt()` (final) | Must be `InProgress`; attempt ≥ MaxAttempts |
| `Queued` or `InProgress` | `Pending` | `RecoverStuckToPending()` | Must be `Queued` or `InProgress` |
| `DeadLettered` | `Pending` | `RequeueFromDeadLetter()` | Must be `DeadLettered` |

When a failed attempt is not the last one, `NextRetryAt` is calculated via `RetryPolicy.CalculateDelayFor(attemptNumber)` and the status returns to `Pending`. `DeliveryProcessorWorker` picks it back up when `NextRetryAt <= now`.

### User Status

```
Active ──► Suspended ──► Active   (Reactivate)
Active ──► Deleted                (soft-delete, terminal)
Suspended ──► Deleted             (soft-delete, terminal)
```

All transitions are idempotent — calling `Suspend()` on an already-suspended user is a no-op.

### API Key Status

```
Active ──► Revoked   (Revoke — NOT idempotent; returns Conflict error if already revoked)
```

### Delivery Attempt Outcome

Each `DeliveryAttempt` is immutable once created. Its `OutcomeStatus` encodes the intent:

| Outcome | Meaning |
|---|---|
| `Succeeded` | HTTP call returned a success response |
| `FailedWillRetry` | HTTP call failed; another attempt is scheduled |
| `FailedFinal` | HTTP call failed; no more attempts will be made |

---

## 6. Key Patterns

### Result\<T\> — Railway-Oriented Error Handling

All domain and application operations return `Result<T>` or `Result` instead of throwing exceptions for expected failures.

```csharp
// Success path
Result<Tenant>.Success(tenant)

// Failure path
Result<Tenant>.Failure(DomainErrors.Tenant.EmptyName)

// Consuming
if (!result.IsSuccess) return Result.Failure(result.Error);
var data = result.Value; // throws InvalidOperationException if failed
```

Controllers unwrap results in `BaseApiController.HandleResult()` and map `ErrorType` to HTTP status codes:

| ErrorType | HTTP Status |
|---|---|
| Validation | 400 |
| NotFound | 404 |
| Conflict | 409 |
| LimitExceeded | 429 |
| Forbidden | 403 |
| (other) | 500 |

### CQRS via MediatR

Commands and queries each live in their own folder under `FlowForge.Application/Features/`. Every handler implements `IRequestHandler<TRequest, Result<T>>`. The `LoggingPipelineBehavior` wraps all handlers with structured entry/exit logging.

### Idempotency

`CreateDeliveryCommand` checks for an existing `WebhookDelivery` by `IdempotencyKey` before creating a new one. If found, it returns the existing delivery ID without side effects.

### SSE Streaming

The `IWebhookDeliveryStreamBroadcaster` abstraction allows any part of the application to publish a `WebhookDeliveryStreamEvent`. The stream endpoint (`GET /api/webhookdelivery/stream`) subscribes per tenant/endpoint/delivery and fans out events to connected clients with a 20-second heartbeat to detect dead connections.

Published event names: `queued`, `in_progress`, `succeeded`, `dead_lettered`, `retry_scheduled`, `recovered`

---

## 7. API Reference

All endpoints requiring authentication expect the `X-Api-Key: fk_live_<64hex>` header.

### Tenants

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/tenant` | None | Create a new tenant |

### API Keys

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/apikey` | None | Create an API key (plain-text returned once) |
| DELETE | `/api/apikey` | None | Revoke an API key |

### Webhook Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/webhookendpoint` | API Key | List all endpoints |
| POST | `/api/webhookendpoint` | API Key | Create an endpoint |
| PATCH | `/api/webhookendpoint/{id}/name` | API Key | Rename endpoint |
| PATCH | `/api/webhookendpoint/{id}/url` | API Key | Change target URL |
| PATCH | `/api/webhookendpoint/{id}/retry-policy` | API Key | Update retry policy |
| PATCH | `/api/webhookendpoint/{id}/subscriptions` | API Key | Replace event subscriptions |
| PATCH | `/api/webhookendpoint/{id}/activate` | API Key | Re-activate endpoint |
| DELETE | `/api/webhookendpoint/{id}` | API Key | Delete endpoint |

### Webhook Deliveries

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/webhookdelivery` | API Key | Create a delivery |
| GET | `/api/webhookdelivery` | API Key | Paginated delivery list |
| GET | `/api/webhookdelivery/all` | API Key | Filtered delivery list |
| GET | `/api/webhookdelivery/{id}` | API Key | Single delivery detail |
| GET | `/api/webhookdelivery/deadletter` | API Key | Dead-lettered deliveries |
| POST | `/api/webhookdelivery/{id}/requeue` | API Key | Requeue a dead-lettered delivery |
| GET | `/api/webhookdelivery/stream` | API Key | SSE stream (`?endpointId=&deliveryId=`) |

### Demo

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/demo` | None | Bootstrap a 24-hour demo tenant; returns `ApiKey` and `ExpiresAt` |

### Test Receivers (Demo Only)

| Method | Path | Description |
|---|---|---|
| POST | `/api/webhookreceiverendpoint/success` | Always returns 200 |
| POST | `/api/webhookreceiverendpoint/fail` | Always returns 500 |

### Health

| Path | Description |
|---|---|
| `/health` | 200 OK if app is running |
| `/health/ready` | JSON with PostgreSQL + Redis probe results |

---

## 8. Background Workers

### `DeliveryProcessorWorker`

Runs every **10 seconds**. Fetches all `Pending` deliveries whose `NextRetryAt <= now`, transitions each to `Queued`, persists, then publishes a `ProcessWebhookDeliveryMessage` to RabbitMQ. Each delivery is processed in an isolated try/catch so one failure doesn't block others. On a cycle-level error the worker backs off for 30 seconds.

### `DeliveryRecoveryWorker`

Runs every **10 seconds**. Finds and recovers stuck deliveries:

- `Queued` for more than **2 minutes** → reset to `Pending`
- `InProgress` for more than **5 minutes** → reset to `Pending`

This guards against crashes mid-flight where the status was advanced but the work was never completed. Recovered deliveries are re-broadcast on the SSE stream with event name `recovered`.

### `DemoCleanupWorker`

Runs every **1 hour**. Finds demo tenants where `DemoExpiresAt < now` and hard-deletes all their data in order: `ApiKeys → WebhookDeliveries → WebhookEndpoints → Tenant → User`. Each tenant is cleaned up independently.

---

## 9. Security

### API Key Authentication Flow

```
Request (X-Api-Key header)
  │
  ▼
ApiKeyAuthenticationHandler
  │
  ▼
ApiKeyValidator.ValidateAsync(plainKey)
  ├─ Hash plain-text with SHA-256
  ├─ Check Redis cache (key: flowforge:apikey:<hash>, TTL: 5 min)
  │   hit  → return cached TenantId + ApiKeyId
  │   miss → query PostgreSQL, populate cache
  │
  ▼
ClaimsIdentity { tenant_id, api_key_id }
```

The plain-text API key is **never stored** — only its SHA-256 hash is persisted. A revoked key's cache entry is explicitly deleted via `IApiKeyValidationCache.RemoveAsync()`.

### Webhook Payload Signing

Every outgoing HTTP request includes:
```
X-FlowForge-Signature: <hmac_sha256_hex(key=SigningSecret, data=payload)>
```

Recipients can verify authenticity by recomputing the signature with their own copy of the secret.

`SigningSecret.ToString()` always returns `***PROTECTED***`, preventing accidental exposure in logs or debug output.

### URL Validation

The `Url` value object rejects:
- Non-HTTPS schemes
- Relative URLs
- `localhost`, `127.x.x.x`, `10.x.x.x`, `192.168.x.x`, and other private ranges

### Polly Resilience (outgoing HTTP)

The `WebhookSender` HTTP client is wrapped with two Polly policies:
- **Retry**: 3 attempts, exponential backoff
- **Circuit Breaker**: opens after 5 consecutive failures, stays open for 30 seconds

---

## 10. Observability

### Structured Logging (Serilog)

Every log entry is enriched with `CorrelationId`, `MachineName`, and `ThreadId`.

The `CorrelationId` is derived from the `X-Correlation-Id` request header (or generated if absent) by `CorrelationIdMiddleware` and propagated through `CorrelationContext` to all downstream services and log entries.

Sinks: Console + rotating daily file at `/logs/log-<date>.txt` (CLEF format).

### Unhandled Exceptions

`ExceptionHandlingMiddleware` catches all unhandled exceptions and returns an RFC 7807 `ProblemDetails` body with `correlationId` attached, so callers can correlate server-side logs.

```json
{
  "status": 500,
  "title": "An unexpected error occurred.",
  "instance": "/api/webhookdelivery",
  "correlationId": "a3f2e1d0-..."
}
```

### Health Checks

```
GET /health/ready

{
  "status": "Healthy",
  "checks": [
    { "name": "postgresql", "status": "Healthy" },
    { "name": "redis",      "status": "Healthy" }
  ]
}
```

---

## 11. Development Setup

### Prerequisites

- .NET 8 SDK
- Docker + Docker Compose

### Steps

```bash
# 1. Start infrastructure
docker-compose up -d

# 2. Restore dependencies
dotnet restore FlowForge.sln

# 3. Apply database migrations
dotnet ef database update -p FlowForge.Persistence -s FlowForge.API

# 4. Run the API
dotnet run --project FlowForge.API/FlowForge.API.csproj
# Swagger: http://localhost:<port>/swagger

# 5. Run tests
dotnet test FlowForge.Domain.Tests/FlowForge.Domain.Tests.csproj
```

### Adding a Migration

```bash
dotnet ef migrations add <MigrationName> -p FlowForge.Persistence -s FlowForge.API
```

> Migrations must never be scaffolded without an explicit request.

### Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=flowforge;..."
  },
  "RabbitMQ": { "HostName": "localhost", "Port": 5672, "UserName": "...", "Password": "..." },
  "Redis":    { "ConnectionString": "localhost:6379" }
}
```

---

*Generated from source — last updated 2026-06-25*
