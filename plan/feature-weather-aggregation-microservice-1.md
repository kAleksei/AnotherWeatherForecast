---
goal: Dockerized .NET Core Weather Forecast Aggregation Microservice with Azure Deployment
version: 1.0
date_created: 2025-11-20
last_updated: 2025-11-20
owner: Development Team
status: Planned
tags: [feature, architecture, infrastructure, microservice, azure, docker]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This implementation plan defines the complete development, testing, and deployment of a production-grade dockerized .NET 8 microservice that aggregates weather forecasts from multiple free API sources. The service implements Clean Architecture with Domain-Driven Design principles, includes comprehensive caching (hybrid memory + Redis), observability (OTEL + structured logging), health checks with degradation support, and automated CI/CD deployment to Azure Container Apps with full Infrastructure as Code (Bicep templates).

## 1. Requirements & Constraints

### Functional Requirements

- **REQ-001**: Single REST API endpoint accepting date (ISO 8601), city, and country as query parameters
- **REQ-002**: Aggregate weather data from minimum 3 free API sources: OpenMeteo, WeatherAPI.com, OpenWeatherMap
- **REQ-003**: Return temperature (Celsius) and humidity (%) from all sources
- **REQ-004**: Support date range: ±7 days from current date (historical and forecast)
- **REQ-005**: Validate date range and return HTTP 400 with clear error message if >7 days
- **REQ-006**: Support source filtering via optional query parameter to return specific sources
- **REQ-007**: Aggregate all sources by default; calculate average temperature and humidity
- **REQ-008**: Graceful degradation: return partial data if 1-2 sources fail; mark unavailable sources
- **REQ-009**: Include metadata in response: cache hit/miss, response time, available sources count
- **REQ-010**: Return HTTP 503 only when all sources are unavailable

### Caching Requirements

- **REQ-011**: Implement two-level hybrid cache: L1 in-memory (15 min TTL), L2 Redis (60 min TTL)
- **REQ-012**: Cache key format: `weather:{city}:{country}:{date:yyyyMMdd}:{sources|all}`
- **REQ-013**: Configurable cache TTL via `appsettings.json`
- **REQ-014**: Support sliding expiration for Redis cache
- **REQ-015**: Fallback to memory-only cache if Redis unavailable

### Observability Requirements

- **REQ-016**: Structured logging with Serilog in JSON format for container logs
- **REQ-017**: OpenTelemetry instrumentation for ASP.NET Core, HttpClient, and Redis
- **REQ-018**: Export telemetry to Azure Application Insights
- **REQ-019**: Custom metrics: source availability rate, cache hit ratio, aggregation response time
- **REQ-020**: Custom spans for each weather source API call and cache operations

### Health Check Requirements

- **REQ-021**: Implement `/health`, `/health/ready`, `/health/live` endpoints
- **REQ-022**: Parallel health checks for all weather sources
- **REQ-023**: Redis cache health check with ping
- **REQ-024**: Return "Degraded" status (HTTP 200) if 1+ sources unavailable but service functional
- **REQ-025**: Return "Unhealthy" status (HTTP 503) only if all sources unavailable

### Architecture Requirements

- **REQ-026**: Clean Architecture with 4 layers: Domain, Application, Infrastructure, API
- **REQ-027**: Domain-Driven Design: entities, value objects, aggregates
- **REQ-028**: Repository pattern with interfaces in Domain layer
- **REQ-029**: Dependency Injection with registration in each layer's `DependencyInjection.cs`
- **REQ-030**: MediatR for application orchestration (optional CQR pattern)
- **REQ-031**: FluentValidation for request validation
- **REQ-032**: AutoMapper for DTO mapping

### Testing Requirements

- **REQ-033**: Unit tests only (no integration tests)
- **REQ-034**: Test coverage target: Domain 100%, Application 90%+, Infrastructure 80%+
- **REQ-035**: Use xUnit, FluentAssertions, NSubstitute, Bogus
- **REQ-036**: Test scenarios: aggregation logic, caching, validation, resilience

### Resilience Requirements

- **REQ-037**: Polly retry policy: 3 attempts with exponential backoff (2^retryAttempt seconds)
- **REQ-038**: Polly circuit breaker: open after 3 consecutive failures, half-open after 30 seconds
- **REQ-039**: Polly timeout: 5 seconds per weather source HTTP request
- **REQ-040**: Independent failure handling per source (no cascading failures)

### Docker Requirements

- **REQ-041**: Multi-stage Dockerfile with build, publish, and runtime stages
- **REQ-042**: Base images: `mcr.microsoft.com/dotnet/sdk:8.0` (build), `mcr.microsoft.com/dotnet/aspnet:8.0` (runtime)
- **REQ-043**: Non-root user execution in container
- **REQ-044**: Health check in Dockerfile: `curl --fail http://localhost:8080/health`
- **REQ-045**: Docker Compose for local development with Redis service
- **REQ-046**: Optimized image size with proper `.dockerignore`

### Azure Deployment Requirements

- **REQ-047**: Deploy to Azure Container Apps (Consumption plan, free tier)
- **REQ-048**: Azure Container Registry (Basic SKU) for image storage
- **REQ-049**: Azure Cache for Redis (Basic C0: 250MB) for distributed caching
- **REQ-050**: Azure Log Analytics + Application Insights for observability
- **REQ-051**: Bicep Infrastructure as Code templates for all Azure resources
- **REQ-052**: Auto-scaling: 0-10 replicas based on HTTP traffic

### CI/CD Requirements

- **REQ-053**: GitHub Actions workflow for build, test, docker push, infrastructure deployment, app deployment
- **REQ-054**: Trigger: push to `main` branch, pull requests (build/test only), manual dispatch
- **REQ-055**: Azure authentication via OIDC (Workload Identity Federation)
- **REQ-056**: Rolling deployment strategy with zero downtime
- **REQ-057**: Post-deployment smoke tests against deployed endpoint
- **REQ-058**: Rollback on failed health checks

### API Documentation Requirements

- **REQ-059**: Swagger/OpenAPI documentation available at `/swagger` (development only)
- **REQ-060**: OpenAPI JSON schema at `/swagger/v1/swagger.json`
- **REQ-061**: Comprehensive descriptions for all endpoints, parameters, and response schemas

### Postman Collection Requirements

- **REQ-062**: Postman Collection v2.1 JSON format
- **REQ-063**: Folders: Health Checks, Weather Forecast, Cache Behavior, Error Scenarios
- **REQ-064**: Environment variables: `baseUrl`, `date`, `city`, `country`
- **REQ-065**: Test assertions for all requests (status code, schema validation, business logic)
- **REQ-066**: Pre-request scripts for dynamic date generation
- **REQ-067**: Newman CLI compatible for automated testing

### Security Constraints

- **SEC-001**: API keys stored in Azure Key Vault (production) and environment variables (development)
- **SEC-002**: No secrets committed to source control
- **SEC-003**: Container runs as non-root user
- **SEC-004**: HTTPS only (enforced by Azure Container Apps)
- **SEC-005**: Vulnerability scanning with Trivy in CI pipeline
- **SEC-006**: Minimal runtime image (no SDK tools in production image)

### Configuration Constraints

- **CON-001**: No API versioning (deliberate v1 decision, document in ADR)
- **CON-002**: No authentication/authorization (future enhancement)
- **CON-003**: No rate limiting (rely on Azure Container Apps throttling)
- **CON-004**: No CORS configuration (future enhancement)
- **CON-005**: Azure free tier limits: 180,000 vCPU-seconds/month, 5GB log ingestion/month
- **CON-006**: OpenMeteo: 10,000 requests/day (no API key)
- **CON-007**: WeatherAPI.com: 1M requests/month free tier (requires API key)
- **CON-008**: OpenWeatherMap: 1,000 requests/day free tier (requires API key)

### Guidelines

- **GUD-001**: Follow LockerOps patterns: domain isolation, repository pattern, pipeline behaviors
- **GUD-002**: Use Conventional Commits format for all commit messages
- **GUD-003**: Create Architecture Decision Records (ADRs) for key decisions
- **GUD-004**: Optimize for readability over cleverness (Clean Code principles)
- **GUD-005**: Prefer composition over inheritance
- **GUD-006**: Keep methods small and single-purpose (SRP)
- **GUD-007**: Use meaningful variable and method names (no abbreviations)

### Patterns to Follow

- **PAT-001**: Options Pattern for strongly-typed configuration with validation
- **PAT-002**: Repository Pattern with interfaces in Domain, implementations in Infrastructure
- **PAT-003**: Factory Pattern for weather source provider instantiation
- **PAT-004**: Strategy Pattern for different weather source implementations
- **PAT-005**: Decorator Pattern for resilience policies (Polly)
- **PAT-006**: Pipeline Behavior Pattern for cross-cutting concerns (logging, validation)
- **PAT-007**: Value Object Pattern for Temperature, Humidity, Location
- **PAT-008**: Aggregate Root Pattern for WeatherForecast entity

## 2. Implementation Steps

### Implementation Phase 1: Project Foundation & Structure

**GOAL-001**: Establish solution structure with Clean Architecture layers, configure dependencies, and set up project references

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create solution file: `dotnet new sln -n WeatherForecast` in `c:\Projects\Home\AnotherWeatherForecast\src\` | | |
| TASK-002 | Create Domain project: `dotnet new classlib -n WeatherForecast.Domain -o src/WeatherForecast.Domain -f net8.0` | | |
| TASK-003 | Create Application project: `dotnet new classlib -n WeatherForecast.Application -o src/WeatherForecast.Application -f net8.0` | | |
| TASK-004 | Create Infrastructure project: `dotnet new classlib -n WeatherForecast.Infrastructure -o src/WeatherForecast.Infrastructure -f net8.0` | | |
| TASK-005 | Create API project: `dotnet new webapi -n WeatherForecast.Api -o src/WeatherForecast.Api -f net8.0` | | |
| TASK-006 | Create test projects: `dotnet new xunit -n WeatherForecast.Domain.Tests -o tests/WeatherForecast.Domain.Tests -f net8.0`, repeat for Application and Infrastructure | | |
| TASK-007 | Add projects to solution: `dotnet sln add` for all 7 projects | | |
| TASK-008 | Configure project references: Application → Domain, Infrastructure → Application/Domain, Api → Application/Infrastructure, Tests → corresponding layer | | |
| TASK-009 | Remove default `Class1.cs` and `WeatherForecast.cs` files from all projects | | |
| TASK-010 | Create folder structure in Domain: `Entities/`, `ValueObjects/`, `Enums/`, `Interfaces/` | | |
| TASK-011 | Create folder structure in Application: `Common/Interfaces/`, `Common/Models/`, `Common/Behaviors/`, `Services/`, `Validators/` | | |
| TASK-012 | Create folder structure in Infrastructure: `ExternalServices/`, `Caching/`, `Configuration/` | | |
| TASK-013 | Create folder structure in API: `Controllers/`, `Middleware/`, `Extensions/` | | |

### Implementation Phase 2: Domain Layer Implementation

**GOAL-002**: Implement domain entities, value objects, enums, and repository interfaces with no external dependencies

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Create `WeatherSourceType` enum in `src/WeatherForecast.Domain/Enums/WeatherSourceType.cs` with values: OpenMeteo, WeatherAPI, OpenWeatherMap | | |
| TASK-015 | Create `Temperature` value object in `src/WeatherForecast.Domain/ValueObjects/Temperature.cs` with Celsius property, validation (range -100 to 60), equality comparison | | |
| TASK-016 | Create `Humidity` value object in `src/WeatherForecast.Domain/ValueObjects/Humidity.cs` with Percent property, validation (range 0-100), equality comparison | | |
| TASK-017 | Create `Location` value object in `src/WeatherForecast.Domain/ValueObjects/Location.cs` with City and Country properties, validation (non-empty, country ISO 3166-1 alpha-2) | | |
| TASK-018 | Create `DateRange` value object in `src/WeatherForecast.Domain/ValueObjects/DateRange.cs` with validation (±7 days from today) | | |
| TASK-019 | Create `ForecastSource` entity in `src/WeatherForecast.Domain/Entities/ForecastSource.cs` with properties: Name, SourceType, Temperature, Humidity, Available, Error, RetrievedAt | | |
| TASK-020 | Create `WeatherForecast` aggregate root in `src/WeatherForecast.Domain/Entities/WeatherForecast.cs` with Location, Date, Sources (List<ForecastSource>), methods: AddSource, CalculateAverage | | |
| TASK-021 | Create `IWeatherRepository` interface in `src/WeatherForecast.Domain/Interfaces/IWeatherRepository.cs` with method: `Task<WeatherForecast?> GetForecastAsync(Location, DateTime, CancellationToken)` | | |
| TASK-022 | Create `CachedWeatherSourceProvider` decorator in `src/WeatherForecast.Infrastructure/Caching/CachedWeatherSourceProvider.cs` to wrap providers with HybridCache functionality | | |

### Implementation Phase 3: Application Layer - DTOs, Interfaces, and Validation

**GOAL-003**: Define application contracts, DTOs, validation rules, and service interfaces

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Create `WeatherForecastRequest` DTO in `src/WeatherForecast.Application/Common/Models/WeatherForecastRequest.cs` with Date, City, Country, Sources (optional) properties | | |
| TASK-024 | Create `ForecastSourceDto` DTO in `src/WeatherForecast.Application/Common/Models/ForecastSourceDto.cs` matching API response schema | | |
| TASK-025 | Create `AggregatedForecastDto` DTO in `src/WeatherForecast.Application/Common/Models/AggregatedForecastDto.cs` with AverageTemperature, AverageHumidity, TemperatureRange | | |
| TASK-026 | Create `WeatherForecastResponse` DTO in `src/WeatherForecast.Application/Common/Models/WeatherForecastResponse.cs` with Location, Date, AggregatedForecast, Sources, Metadata | | |
| TASK-027 | Create `ResponseMetadata` DTO in `src/WeatherForecast.Application/Common/Models/ResponseMetadata.cs` with TotalSources, AvailableSources, CacheHit, ResponseTimeMs | | |
| TASK-028 | Create `IWeatherSourceProvider` interface in `src/WeatherForecast.Application/Common/Interfaces/IWeatherSourceProvider.cs` with methods: `Task<ForecastSource> GetForecastAsync`, `WeatherSourceType SourceType` | | |
| TASK-029 | Create `IWeatherAggregationService` interface in `src/WeatherForecast.Application/Common/Interfaces/IWeatherAggregationService.cs` with method: `Task<WeatherForecastResponse> GetAggregatedForecastAsync` | | |
| TASK-030 | Create `WeatherForecastRequestValidator` in `src/WeatherForecast.Application/Validators/WeatherForecastRequestValidator.cs` using FluentValidation: validate date range (±7 days), required fields, country code format | | |
| TASK-031 | Install NuGet packages in Application project: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `AutoMapper`, `MediatR` | | |
| TASK-032 | Create AutoMapper profile `MappingProfile` in `src/WeatherForecast.Application/Common/Mappings/MappingProfile.cs` for Entity ↔ DTO mappings | | |

### Implementation Phase 4: Application Layer - Services and Behaviors

**GOAL-004**: Implement core business logic for weather aggregation, caching, and cross-cutting concerns

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-033 | Create `CacheKeyGenerator` in `src/WeatherForecast.Application/Services/CacheKeyGenerator.cs` with method: `GenerateKey(city, country, date, sources)` format: `weather:{city}:{country}:{yyyyMMdd}:{sources}` | | |
| TASK-034 | Create `WeatherAggregationService` in `src/WeatherForecast.Application/Services/WeatherAggregationService.cs` implementing `IWeatherAggregationService` | | |
| TASK-035 | Implement `GetAggregatedForecastAsync` method: (1) Generate cache key (2) Check cache (3) If miss, call all providers in parallel (4) Aggregate results (5) Cache response (6) Return DTO | | |
| TASK-036 | Implement source filtering logic in `WeatherAggregationService`: if sources parameter provided, filter provider list | | |
| TASK-037 | Implement aggregation logic: calculate average temperature/humidity from available sources, handle partial failures | | |
| TASK-038 | Create `ValidationBehavior<TRequest, TResponse>` in `src/WeatherForecast.Application/Common/Behaviors/ValidationBehavior.cs` using FluentValidation with MediatR pipeline | | |
| TASK-039 | Create `LoggingBehavior<TRequest, TResponse>` in `src/WeatherForecast.Application/Common/Behaviors/LoggingBehavior.cs` for request/response logging with timing | | |
| TASK-040 | Create `DependencyInjection.cs` in `src/WeatherForecast.Application/DependencyInjection.cs` with extension method `AddApplication(IServiceCollection)` registering services, validators, AutoMapper, MediatR behaviors | | |

### Implementation Phase 5: Infrastructure Layer - External API Providers

**GOAL-005**: Implement weather source providers with HTTP clients, resilience policies, and error handling

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-041 | Install NuGet packages in Infrastructure: `Microsoft.Extensions.Http.Polly`, `StackExchange.Redis`, `Microsoft.Extensions.Caching.Memory`, `Polly` | | |
| TASK-042 | Create `WeatherSourceOptions` in `src/WeatherForecast.Infrastructure/Configuration/WeatherSourceOptions.cs` with BaseUrl, ApiKey, TimeoutSeconds, CacheDurationMinutes properties | | |
| TASK-043 | Create `OpenMeteoProvider` in `src/WeatherForecast.Infrastructure/ExternalServices/OpenMeteoProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-044 | Implement OpenMeteo API integration: endpoint `https://api.open-meteo.com/v1/forecast`, parameters: latitude, longitude (from geocoding), date, temperature_2m, relative_humidity_2m | | |
| TASK-045 | Create `WeatherApiProvider` in `src/WeatherForecast.Infrastructure/ExternalServices/WeatherApiProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-046 | Implement WeatherAPI.com integration: endpoint `https://api.weatherapi.com/v1/{forecast or history}.json`, API key authentication, parse JSON response | | |
| TASK-047 | Create `OpenWeatherMapProvider` in `src/WeatherForecast.Infrastructure/ExternalServices/OpenWeatherMapProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-048 | Implement OpenWeatherMap integration: endpoint `https://api.openweathermap.org/data/2.5/forecast` or `onecall/timemachine`, API key authentication | | |
| TASK-049 | Configure Polly policies for all HTTP clients: Retry (3 attempts, exponential backoff), Circuit Breaker (3 failures, 30s break), Timeout (5s) | | |
| TASK-050 | Implement error handling in each provider: catch HttpRequestException, TimeoutException, map to ForecastSource with Available=false and Error message | | |
| TASK-051 | Add structured logging in each provider: log request start, success, failure with correlation IDs | | |

### Implementation Phase 6: Infrastructure Layer - Caching with Decorator Pattern

**GOAL-006**: Implement caching at provider level using CachedWeatherSourceProvider decorator with HybridCache

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-052 | Add `Microsoft.Extensions.Caching.Hybrid` NuGet package to Infrastructure project | | |
| TASK-053 | Create `CachedWeatherSourceProvider` decorator in `src/WeatherForecast.Infrastructure/Caching/CachedWeatherSourceProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-054 | Implement caching logic using HybridCache with per-provider TTL from `WeatherSourceOptions.CacheDurationMinutes` | | |
| TASK-055 | Generate cache keys with format: `weather:{SourceName}:{City}:{Country}:{yyyyMMdd}` | | |
| TASK-056 | Register HybridCache in DependencyInjection: `services.AddHybridCache()` | | |
| TASK-057 | Wrap each weather provider with `CachedWeatherSourceProvider` in DI registration | | |
| TASK-058 | Remove `ICacheRepository` dependency from `WeatherAggregationService` | | |
### Implementation Phase 7: Infrastructure Layer - Health Checks

**GOAL-007**: Implement comprehensive health checks with degradation support

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-059 | Create `WeatherSourceHealthCheck` in `src/WeatherForecast.Infrastructure/ExternalServices/WeatherSourceHealthCheck.cs` implementing `IHealthCheck` | | |
| TASK-060 | Implement parallel health checks for all weather sources: test HTTP connectivity, API validity | | |
| TASK-061 | Implement health check logic: Healthy (all sources OK), Degraded (1+ sources down), Unhealthy (all sources down) | | |
| TASK-062 | Create `DependencyInjection.cs` in `src/WeatherForecast.Infrastructure/DependencyInjection.cs` (if not exists) to register health checks | | |
| TASK-063 | Register weather providers as singletons with caching decorators in `DependencyInjection.cs` | | |
| TASK-064 | Register HybridCache: `services.AddHybridCache()` | | |
| TASK-065 | Register health checks: `services.AddHealthChecks().AddCheck<WeatherSourceHealthCheck>()` | | |
| TASK-060 | Implement parallel health checks for all weather sources: test HTTP connectivity, API key validity | | |
| TASK-061 | Implement health check logic: Healthy (all sources OK), Degraded (1+ sources down), Unhealthy (all sources down) | | |
| TASK-062 | Create `RedisHealthCheck` in `src/WeatherForecast.Infrastructure/Caching/RedisHealthCheck.cs`: ping Redis, return Healthy/Unhealthy | | |
| TASK-063 | Create `DependencyInjection.cs` in `src/WeatherForecast.Infrastructure/DependencyInjection.cs` with `AddInfrastructure(IServiceCollection, IConfiguration)` extension method | | |
| TASK-064 | Register weather providers as singletons with named HttpClients in `DependencyInjection.cs` | | |
| TASK-065 | Register Redis connection: `services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString))` | | |
| TASK-066 | Register memory cache: `services.AddMemoryCache(options => options.SizeLimit = configSizeMB)` | | |
| TASK-067 | Register health checks: `services.AddHealthChecks().AddCheck<WeatherSourceHealthCheck>().AddCheck<RedisHealthCheck>()` | | |

### Implementation Phase 7: API Layer - Controllers, Middleware, and Configuration

**GOAL-007**: Implement REST API endpoint, exception handling middleware, and API configuration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-068 | Install NuGet packages in API project: `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Formatting.Compact`, `OpenTelemetry.Exporter.Console`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `Azure.Monitor.OpenTelemetry.AspNetCore` | | |
| TASK-069 | Create `WeatherController` in `src/WeatherForecast.Api/Controllers/WeatherController.cs` inheriting `ControllerBase` with `[ApiController]` and `[Route("api/weather")]` | | |
| TASK-070 | Implement `GetForecast` action: `[HttpGet("forecast")]` accepting query parameters: date, city, country, sources (optional) | | |
| TASK-071 | Inject `IWeatherAggregationService` and `IValidator<WeatherForecastRequest>` into controller constructor | | |
| TASK-072 | Implement validation in action: call validator, return `BadRequest(errors)` if validation fails | | |
| TASK-073 | Call aggregation service, measure response time using `Stopwatch`, return `Ok(response)` | | |
| TASK-074 | Add XML documentation comments for Swagger: `/// <summary>`, `/// <param>`, `/// <response>` | | |
| TASK-075 | Create `ExceptionHandlingMiddleware` in `src/WeatherForecast.Api/Middleware/ExceptionHandlingMiddleware.cs` implementing global exception handler | | |
| TASK-076 | Implement exception handling: catch all exceptions, log with Serilog, return ProblemDetails (RFC 7807) with status 500 | | |
| TASK-077 | Create `RequestLoggingMiddleware` in `src/WeatherForecast.Api/Middleware/RequestLoggingMiddleware.cs` for structured HTTP request/response logging | | |
| TASK-078 | Log request path, method, query string, response status, duration for each HTTP request | | |
| TASK-079 | Create `OpenTelemetryExtensions` in `src/WeatherForecast.Api/Extensions/OpenTelemetryExtensions.cs` with `AddOpenTelemetryObservability` extension method | | |
| TASK-080 | Configure OTEL instrumentation: ASP.NET Core, HttpClient, Redis (StackExchange.Redis) | | |
| TASK-081 | Configure OTEL exporters: Azure Monitor (production), Console (development) based on environment | | |
| TASK-082 | Create `HealthCheckExtensions` in `src/WeatherForecast.Api/Extensions/HealthCheckExtensions.cs` with `AddHealthCheckEndpoints` extension method | | |
| TASK-083 | Map health check endpoints in extension: `/health` (detailed), `/health/ready` (readiness), `/health/live` (liveness) | | |

### Implementation Phase 8: API Layer - Program.cs and Configuration Files

**GOAL-008**: Configure application startup, dependency injection, middleware pipeline, and settings

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-084 | Configure Serilog in `src/WeatherForecast.Api/Program.cs`: read from configuration, use CompactJsonFormatter for console, enrich with machine name | | |
| TASK-085 | Configure `WebApplicationBuilder`: `builder.Host.UseSerilog()` for logging | | |
| TASK-086 | Register services: `builder.Services.AddApplication()`, `builder.Services.AddInfrastructure(builder.Configuration)` | | |
| TASK-087 | Register controllers: `builder.Services.AddControllers()` | | |
| TASK-088 | Register Swagger: `builder.Services.AddEndpointsApiExplorer()`, `builder.Services.AddSwaggerGen(options => configure XML comments)` | | |
| TASK-089 | Register OpenTelemetry: `builder.Services.AddOpenTelemetryObservability(builder.Configuration)` | | |
| TASK-090 | Build app: `var app = builder.Build();` | | |
| TASK-091 | Configure middleware pipeline: (1) ExceptionHandling (2) RequestLogging (3) Swagger (dev only) (4) HTTPS redirection (5) MapControllers (6) Health checks | | |
| TASK-092 | Run app: `app.Run();` | | |
| TASK-093 | Create `appsettings.json` in `src/WeatherForecast.Api/appsettings.json` with sections: Logging, WeatherSources, CacheSettings, ConnectionStrings, HealthCheckSettings | | |
| TASK-094 | Configure WeatherSources section: OpenMeteo (BaseUrl, TimeoutSeconds), WeatherAPI (BaseUrl, ApiKey, TimeoutSeconds), OpenWeatherMap (BaseUrl, ApiKey, TimeoutSeconds) | | |
| TASK-095 | Configure CacheSettings section: MemoryCacheDurationMinutes=15, RedisCacheDurationMinutes=60, EnableDistributedCache=true, SlidingExpiration=true | | |
| TASK-096 | Configure ConnectionStrings section: Redis="localhost:6379" | | |
| TASK-097 | Configure Serilog section: MinimumLevel (Default=Information), WriteTo (Console with CompactJsonFormatter), Enrich (FromLogContext, WithMachineName) | | |
| TASK-098 | Create `appsettings.Development.json` overriding: ConnectionStrings:Redis, ApplicationInsights disabled | | |
| TASK-099 | Configure project properties: enable XML documentation generation for Swagger, set Docker support | | |

### Implementation Phase 9: Unit Tests - Domain and Application Layers

**GOAL-009**: Implement comprehensive unit tests for domain entities, value objects, services, and validators

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-100 | Install NuGet packages in test projects: `xunit`, `FluentAssertions`, `NSubstitute`, `Bogus`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio` | | |
| TASK-101 | Create `TemperatureTests` in `tests/WeatherForecast.Domain.Tests/ValueObjects/TemperatureTests.cs` | | |
| TASK-102 | Test Temperature value object: valid range (-100 to 60), invalid ranges throw exception, equality comparison | | |
| TASK-103 | Create `HumidityTests` in `tests/WeatherForecast.Domain.Tests/ValueObjects/HumidityTests.cs` | | |
| TASK-104 | Test Humidity value object: valid range (0-100), invalid ranges throw exception, equality comparison | | |
| TASK-105 | Create `LocationTests` in `tests/WeatherForecast.Domain.Tests/ValueObjects/LocationTests.cs` | | |
| TASK-106 | Test Location value object: valid city/country, empty values throw exception, country code format validation | | |
| TASK-107 | Create `DateRangeTests` in `tests/WeatherForecast.Domain.Tests/ValueObjects/DateRangeTests.cs` | | |
| TASK-108 | Test DateRange value object: ±7 days validation, dates beyond range throw exception | | |
| TASK-109 | Create `WeatherForecastTests` in `tests/WeatherForecast.Domain.Tests/Entities/WeatherForecastTests.cs` | | |
| TASK-110 | Test WeatherForecast aggregate: AddSource method, CalculateAverage with all sources, CalculateAverage with partial sources | | |
| TASK-111 | Create `WeatherAggregationServiceTests` in `tests/WeatherForecast.Application.Tests/Services/WeatherAggregationServiceTests.cs` | | |
| TASK-112 | Mock dependencies: `IWeatherSourceProvider[]`, `ICacheRepository`, `ILogger` using NSubstitute | | |
| TASK-113 | Test cache hit scenario: mock cache returns value, verify providers not called | | |
| TASK-114 | Test cache miss scenario: mock cache returns null, verify providers called, verify cache set | | |
| TASK-115 | Test all sources available: verify aggregation calculates correct average | | |
| TASK-116 | Test partial sources available: verify aggregation handles 1-2 failures, calculates average from available | | |
| TASK-117 | Test all sources unavailable: verify service returns appropriate response with all sources marked unavailable | | |
| TASK-118 | Test source filtering: provide sources parameter, verify only specified providers called | | |
| TASK-119 | Create `WeatherForecastRequestValidatorTests` in `tests/WeatherForecast.Application.Tests/Validators/WeatherForecastRequestValidatorTests.cs` | | |
| TASK-120 | Test valid request: date within range, required fields present | | |
| TASK-121 | Test invalid date range: date >7 days future, date >7 days past | | |
| TASK-122 | Test missing required fields: city, country | | |
| TASK-123 | Test invalid country code format | | |

### Implementation Phase 10: Unit Tests - Infrastructure Layer

**GOAL-010**: Implement unit tests for weather providers, caching service, and health checks

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-124 | Create `OpenMeteoProviderTests` in `tests/WeatherForecast.Infrastructure.Tests/ExternalServices/OpenMeteoProviderTests.cs` | | |
| TASK-125 | Mock HttpClient using `HttpMessageHandler` stub, configure mock responses | | |
| TASK-126 | Test successful API response: verify ForecastSource populated correctly, Available=true | | |
| TASK-127 | Test HTTP failure: mock throws HttpRequestException, verify Available=false, Error populated | | |
| TASK-128 | Test timeout: mock delays >5s, verify timeout exception handled, Available=false | | |
| TASK-129 | Repeat TASK-124 to TASK-128 for `WeatherApiProviderTests` and `OpenWeatherMapProviderTests` | | |
| TASK-130 | Create `HybridCacheServiceTests` in `tests/WeatherForecast.Infrastructure.Tests/Caching/HybridCacheServiceTests.cs` | | |
| TASK-131 | Mock `IMemoryCache` and `IConnectionMultiplexer` using NSubstitute | | |
| TASK-132 | Test GetAsync with L1 hit: verify L2 not called | | |
| TASK-133 | Test GetAsync with L1 miss, L2 hit: verify L1 populated with L2 value | | |
| TASK-134 | Test GetAsync with L1 and L2 miss: verify null returned | | |
| TASK-135 | Test SetAsync: verify both L1 and L2 called with correct TTL | | |
| TASK-136 | Test SetAsync with Redis disabled: verify only L1 called | | |
| TASK-137 | Test Redis unavailable: verify fallback to L1 only, no exceptions thrown | | |

### Implementation Phase 11: Docker Configuration

**GOAL-011**: Create optimized Docker configuration for local development and production deployment

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-138 | Create `Dockerfile` in `c:\Projects\Home\AnotherWeatherForecast\Dockerfile` (multi-stage) | | |
| TASK-139 | Build stage: FROM `mcr.microsoft.com/dotnet/sdk:8.0` AS build, WORKDIR /src | | |
| TASK-140 | Copy solution and csproj files, RUN `dotnet restore src/WeatherForecast.Api/WeatherForecast.Api.csproj` | | |
| TASK-141 | Copy all source code, RUN `dotnet build -c Release -o /app/build` | | |
| TASK-142 | Publish stage: FROM build AS publish, RUN `dotnet publish -c Release -o /app/publish /p:UseAppHost=false` | | |
| TASK-143 | Runtime stage: FROM `mcr.microsoft.com/dotnet/aspnet:8.0` AS final, WORKDIR /app | | |
| TASK-144 | Create non-root user: RUN `adduser --disabled-password --gecos '' appuser && chown -R appuser /app`, USER appuser | | |
| TASK-145 | COPY from publish stage, EXPOSE 8080 | | |
| TASK-146 | Add HEALTHCHECK: `--interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1` | | |
| TASK-147 | ENTRYPOINT ["dotnet", "WeatherForecast.Api.dll"] | | |
| TASK-148 | Create `.dockerignore` in `c:\Projects\Home\AnotherWeatherForecast\.dockerignore` | | |
| TASK-149 | Add to .dockerignore: `**/.git`, `**/.vs`, `**/.vscode`, `**/bin`, `**/obj`, `**/.gitignore`, `**/docker-compose*`, `**/Dockerfile*`, `**/*.md`, `**/secrets.json` | | |
| TASK-150 | Create `docker-compose.yml` in `c:\Projects\Home\AnotherWeatherForecast\docker-compose.yml` | | |
| TASK-151 | Configure api service: build context, ports 5000:8080, environment variables (ASPNETCORE_ENVIRONMENT, ConnectionStrings__Redis, WeatherSources keys), depends_on redis | | |
| TASK-152 | Configure redis service: image `redis:7-alpine`, ports 6379:6379, volume `redis-data:/data` | | |
| TASK-153 | Test Docker build locally: `docker build -t weatherforecast:local .` | | |
| TASK-154 | Test docker-compose locally: `docker-compose up`, verify API accessible at http://localhost:5000/health | | |

### Implementation Phase 12: Azure Infrastructure as Code (Bicep)

**GOAL-012**: Create Bicep templates for all Azure resources with parameterization for dev/prod environments

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-155 | Create directory structure: `c:\Projects\Home\AnotherWeatherForecast\infra\`, `infra\modules\`, `infra\parameters\` | | |
| TASK-156 | Create `main.bicep` in `infra\main.bicep` with parameters: environmentName, location, appName, redisEnabled | | |
| TASK-157 | Create `container-registry.bicep` in `infra\modules\container-registry.bicep`: Azure Container Registry (Basic SKU), admin enabled, output: loginServer, name | | |
| TASK-158 | Create `log-analytics.bicep` in `infra\modules\log-analytics.bicep`: Log Analytics Workspace, retention 90 days, output: workspaceId | | |
| TASK-159 | Create `app-insights.bicep` in `infra\modules\app-insights.bicep`: Application Insights linked to Log Analytics, output: connectionString, instrumentationKey | | |
| TASK-160 | Create `redis.bicep` in `infra\modules\redis.bicep`: Azure Cache for Redis (Basic C0 250MB), non-SSL port enabled for testing, output: connectionString | | |
| TASK-161 | Create `container-app.bicep` in `infra\modules\container-app.bicep`: Container Apps Environment, Container App with image reference, environment variables, scaling rules (0-10 replicas) | | |
| TASK-162 | Configure Container App environment variables in Bicep: ConnectionStrings__Redis (from redis output), WeatherSources keys (from parameters), ApplicationInsights__ConnectionString | | |
| TASK-163 | Configure Container App ingress: external=true, targetPort=8080, allowInsecure=false (HTTPS) | | |
| TASK-164 | Configure Container App scaling: minReplicas=0, maxReplicas=10, rules based on HTTP concurrent requests (10) | | |
| TASK-165 | In main.bicep, orchestrate modules: (1) Log Analytics (2) App Insights (3) Container Registry (4) Redis (if enabled) (5) Container App | | |
| TASK-166 | Output from main.bicep: containerAppFqdn, containerRegistryLoginServer, appInsightsConnectionString | | |
| TASK-167 | Create `dev.parameters.json` in `infra\parameters\dev.parameters.json` with values: environmentName=dev, location=eastus, appName=weatherforecast-dev, redisEnabled=false (use memory only) | | |
| TASK-168 | Create `prod.parameters.json` in `infra\parameters\prod.parameters.json` with values: environmentName=prod, location=eastus, appName=weatherforecast-prod, redisEnabled=true | | |

### Implementation Phase 13: CI/CD Pipeline (GitHub Actions)

**GOAL-013**: Implement automated build, test, docker push, infrastructure deployment, and application deployment pipeline

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-169 | Create `.github\workflows\deploy.yml` in `c:\Projects\Home\AnotherWeatherForecast\.github\workflows\deploy.yml` | | |
| TASK-170 | Configure workflow triggers: push to main branch, pull_request (build/test only), workflow_dispatch | | |
| TASK-171 | Define workflow environment variables: AZURE_RESOURCE_GROUP, AZURE_LOCATION, APP_NAME, REGISTRY_NAME | | |
| TASK-172 | Create Job 1 "build-and-test": runs-on ubuntu-latest, steps: checkout, setup .NET 8, restore, build, test with coverage | | |
| TASK-173 | Upload test results and coverage artifacts in build-and-test job | | |
| TASK-174 | Create Job 2 "docker-build-push": needs build-and-test, runs only on push to main | | |
| TASK-175 | Docker build-push steps: checkout, Azure login (OIDC), ACR login, Docker build with tags (git SHA + latest), Docker push | | |
| TASK-176 | Create Job 3 "infrastructure-deploy": needs build-and-test, runs only on push to main | | |
| TASK-177 | Infrastructure deploy steps: checkout, Azure login, deploy Bicep (az deployment group create), capture outputs | | |
| TASK-178 | Create Job 4 "app-deploy": needs [docker-build-push, infrastructure-deploy], runs only on push to main | | |
| TASK-179 | App deploy steps: Azure login, update Container App with new image (az containerapp update), wait for deployment | | |
| TASK-180 | Create Job 5 "post-deploy-tests": needs app-deploy, runs only on push to main | | |
| TASK-181 | Post-deploy test steps: health check curl (retry 5 times), smoke test GET /api/weather/forecast with valid parameters, validate response schema | | |
| TASK-182 | Configure GitHub secrets requirements in README: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID, WEATHERAPI_KEY, OPENWEATHERMAP_KEY | | |
| TASK-183 | Add rollback step in app-deploy: if health check fails after deployment, rollback to previous revision | | |

### Implementation Phase 14: Documentation - README and ADRs

**GOAL-014**: Create comprehensive project documentation including setup, usage, deployment, and architecture decisions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-184 | Create `README.md` in `c:\Projects\Home\AnotherWeatherForecast\README.md` with sections: Overview, Features, Architecture, Prerequisites, Local Setup, API Usage, Testing, Docker, Azure Deployment, Environment Variables, Troubleshooting | | |
| TASK-185 | Add architecture diagram to README (mermaid or image): show Clean Architecture layers, data flow, external dependencies | | |
| TASK-186 | Document prerequisites: .NET 8 SDK, Docker Desktop, Azure CLI, Visual Studio 2022 / VS Code | | |
| TASK-187 | Document local setup steps: clone repo, obtain API keys, create `.env` file, run docker-compose, access Swagger | | |
| TASK-188 | Document API usage: provide curl examples for (1) aggregated forecast (2) filtered sources (3) historical date (4) error scenarios | | |
| TASK-189 | Document testing: run all tests command, run coverage report, interpret results | | |
| TASK-190 | Document Docker: build image, run container, environment variables reference | | |
| TASK-191 | Document Azure deployment: prerequisites (Azure subscription, service principal), infrastructure deployment command, app deployment via GitHub Actions, access deployed API | | |
| TASK-192 | Create environment variables reference table: name, description, required, default, example | | |
| TASK-193 | Add troubleshooting section: common issues (Redis connection, API key invalid, health check degraded, container not starting) | | |
| TASK-194 | Create `docs\adr\` directory for Architecture Decision Records | | |
| TASK-195 | Create ADR-001: "Use Clean Architecture with DDD" - context, decision, consequences, alternatives (layered, hexagonal) | | |
| TASK-196 | Create ADR-002: "Two-Level Hybrid Caching Strategy" - context (performance, cost), decision (memory + Redis), consequences, alternatives (Redis only, memory only) | | |
| TASK-197 | Create ADR-003: "No API Versioning in V1" - context, decision (defer to v2), consequences (breaking changes require new endpoints), alternatives (URL versioning, header versioning) | | |
| TASK-198 | Create ADR-004: "Azure Container Apps vs App Service" - context (containerized workload), decision (Container Apps for native container support, auto-scaling), consequences, alternatives (App Service, AKS) | | |
| TASK-199 | Create ADR-005: "Free Weather API Sources Selection" - context (cost constraint), decision (OpenMeteo, WeatherAPI, OpenWeatherMap), consequences (rate limits, feature limitations), alternatives (paid APIs) | | |

### Implementation Phase 15: Postman Collection and Final Integration

**GOAL-015**: Create comprehensive Postman collection with tests and finalize end-to-end integration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-200 | Create Postman Collection v2.1 JSON file: `c:\Projects\Home\AnotherWeatherForecast\postman\WeatherForecast.postman_collection.json` | | |
| TASK-201 | Configure collection variables: `baseUrl` (http://localhost:5000), `date` ({{$isoTimestamp}}), `city` (London), `country` (GB) | | |
| TASK-202 | Create folder "Health Checks" with requests: GET /health, GET /health/ready, GET /health/live | | |
| TASK-203 | Add tests to health check requests: validate status 200, validate response schema (status, checks array) | | |
| TASK-204 | Create folder "Weather Forecast" with request "GET Aggregated Forecast" - URL: `{{baseUrl}}/api/weather/forecast?date={{date}}&city={{city}}&country={{country}}` | | |
| TASK-205 | Add tests to Aggregated Forecast: validate status 200, validate response schema (location, date, aggregatedForecast, sources array, metadata), validate aggregatedForecast.averageTemperatureCelsius is number, validate sources.length >= 3 | | |
| TASK-206 | Create request "GET Filtered by Source" with sources parameter: `sources=OpenMeteo` | | |
| TASK-207 | Add tests to Filtered by Source: validate sources array length = 1, validate sources[0].name = "OpenMeteo" | | |
| TASK-208 | Create request "GET Multiple Sources" with sources parameter: `sources=OpenMeteo,WeatherAPI` | | |
| TASK-209 | Add tests to Multiple Sources: validate sources array length = 2 | | |
| TASK-210 | Create request "GET Future Date" with pre-request script: set date = today + 3 days | | |
| TASK-211 | Create request "GET Historical Date" with pre-request script: set date = today - 3 days | | |
| TASK-212 | Create request "GET Invalid Date" with date = today + 10 days | | |
| TASK-213 | Add tests to Invalid Date: validate status 400, validate error message contains "7 days" | | |
| TASK-214 | Create folder "Cache Behavior" with request "GET First Request" - same as Aggregated Forecast | | |
| TASK-215 | Add tests to First Request: validate metadata.cacheHit = false | | |
| TASK-216 | Create request "GET Second Request" - duplicate of First Request | | |
| TASK-217 | Add tests to Second Request: validate metadata.cacheHit = true, validate responseTimeMs < first request | | |
| TASK-218 | Create folder "Error Scenarios" with request "GET Missing Required Parameters" - omit city parameter | | |
| TASK-219 | Add tests to Missing Required Parameters: validate status 400, validate errors object contains "city" | | |
| TASK-220 | Create request "GET Invalid Country Code" with country = "INVALID" | | |
| TASK-221 | Create request "GET Malformed Date Format" with date = "2025/11/20" (wrong format) | | |
| TASK-222 | Create Postman environment file: `postman\WeatherForecast-Dev.postman_environment.json` with baseUrl = http://localhost:5000 | | |
| TASK-223 | Create Postman environment file: `postman\WeatherForecast-Prod.postman_environment.json` with baseUrl = https://{{AZURE_FQDN}} | | |
| TASK-224 | Add Newman CLI test script to README: `newman run postman/WeatherForecast.postman_collection.json -e postman/WeatherForecast-Dev.postman_environment.json` | | |

### Implementation Phase 16: Configuration and Final Touches

**GOAL-016**: Create configuration files, environment templates, and perform final validation

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-225 | Create `.env.example` in `c:\Projects\Home\AnotherWeatherForecast\.env.example` | | |
| TASK-226 | Add to .env.example: `WEATHERAPI_KEY=your_key_here`, `OPENWEATHERMAP_KEY=your_key_here`, `REDIS_CONNECTION_STRING=localhost:6379`, `ASPNETCORE_ENVIRONMENT=Development`, `APPLICATIONINSIGHTS_CONNECTION_STRING=` (optional) | | |
| TASK-227 | Add comments to .env.example explaining how to obtain each API key | | |
| TASK-228 | Create `.gitignore` if not exists, ensure it includes: `.env`, `bin/`, `obj/`, `.vs/`, `.vscode/`, `*.user`, `secrets.json` | | |
| TASK-229 | Verify all XML documentation comments are present in API controllers for Swagger generation | | |
| TASK-230 | Run `dotnet format` across entire solution to ensure consistent code formatting | | |
| TASK-231 | Run all unit tests: `dotnet test --configuration Release --logger "console;verbosity=detailed"` - verify 100% pass | | |
| TASK-232 | Generate test coverage report: `dotnet test --collect:"XPlat Code Coverage"` - verify coverage targets met (Domain 100%, Application 90%+, Infrastructure 80%+) | | |
| TASK-233 | Build Docker image: `docker build -t weatherforecast:1.0 .` - verify successful build | | |
| TASK-234 | Run Docker container locally: `docker run -p 5000:8080 -e ConnectionStrings__Redis=host.docker.internal:6379 weatherforecast:1.0` - verify health endpoint responds | | |
| TASK-235 | Test Swagger UI: navigate to http://localhost:5000/swagger - verify all endpoints documented with examples | | |
| TASK-236 | Import Postman collection, run all requests against local Docker container - verify all tests pass | | |
| TASK-237 | Validate Bicep templates: `az bicep build --file infra/main.bicep` - verify no syntax errors | | |
| TASK-238 | Perform security scan: run `dotnet list package --vulnerable` - verify no vulnerable dependencies | | |
| TASK-239 | Add LICENSE file (MIT or Apache 2.0) in root directory | | |
| TASK-240 | Create CONTRIBUTING.md with contribution guidelines, code of conduct, pull request process | | |

### Implementation Phase 17: Azure Deployment and Validation

**GOAL-017**: Deploy infrastructure and application to Azure, validate end-to-end functionality in production

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-241 | Set up Azure service principal with OIDC for GitHub Actions: `az ad sp create-for-rbac --name weatherforecast-github --role contributor --scopes /subscriptions/{subscription-id}` | | |
| TASK-242 | Configure GitHub secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID from service principal output | | |
| TASK-243 | Configure GitHub secrets for API keys: WEATHERAPI_KEY, OPENWEATHERMAP_KEY | | |
| TASK-244 | Create Azure Resource Group: `az group create --name rg-weatherforecast-prod --location eastus` | | |
| TASK-245 | Deploy infrastructure via Bicep: `az deployment group create --resource-group rg-weatherforecast-prod --template-file infra/main.bicep --parameters infra/parameters/prod.parameters.json` | | |
| TASK-246 | Capture Bicep outputs: containerRegistryLoginServer, appInsightsConnectionString, containerAppFqdn | | |
| TASK-247 | Build Docker image with Azure Container Registry: `az acr build --registry {ACR_NAME} --image weatherforecast:latest --image weatherforecast:1.0 .` | | |
| TASK-248 | Update Container App with image and environment variables: `az containerapp update --name weatherforecast-prod --resource-group rg-weatherforecast-prod --image {ACR_LOGIN_SERVER}/weatherforecast:latest` | | |
| TASK-249 | Configure environment variables in Container App: ConnectionStrings__Redis, WeatherSources API keys, ApplicationInsights connection string | | |
| TASK-250 | Test health endpoint: `curl https://{CONTAINER_APP_FQDN}/health` - verify status Healthy | | |
| TASK-251 | Test weather forecast endpoint: `curl "https://{CONTAINER_APP_FQDN}/api/weather/forecast?date=2025-11-20&city=London&country=GB"` - verify aggregated response | | |
| TASK-252 | Import Postman collection, update Prod environment with CONTAINER_APP_FQDN, run all requests - verify all tests pass | | |
| TASK-253 | Verify OpenTelemetry data in Azure Application Insights: check request traces, dependencies, custom metrics | | |
| TASK-254 | Verify structured logs in Log Analytics: query logs with KQL, verify request/response logs present | | |
| TASK-255 | Test auto-scaling: generate load using Apache Bench or k6, verify Container App scales up to 10 replicas | | |
| TASK-256 | Test cache behavior: make same request twice, verify second request has lower response time and cacheHit=true | | |
| TASK-257 | Test graceful degradation: disable one weather source API key, verify service returns degraded health but continues serving partial data | | |
| TASK-258 | Commit all changes to Git: `git add .`, `git commit -m "feat: complete weather forecast microservice with Azure deployment"` | | |
| TASK-259 | Push to GitHub main branch: `git push origin main` - verify GitHub Actions workflow triggers and completes successfully | | |
| TASK-260 | Verify CI/CD pipeline: check workflow run in GitHub Actions, verify all jobs pass (build, test, docker push, infrastructure deploy, app deploy, post-deploy tests) | | |
### Implementation Phase 16: Configuration and Final Touches

**GOAL-016**: Create configuration files, environment templates, and perform final validation

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-225 | Create `.env.example` in `c:\Projects\Home\AnotherWeatherForecast\.env.example` | | |
| TASK-226 | Add to .env.example: `WEATHERAPI_KEY=your_key_here`, `OPENWEATHERMAP_KEY=your_key_here`, `REDIS_CONNECTION_STRING=localhost:6379`, `ASPNETCORE_ENVIRONMENT=Development`, `APPLICATIONINSIGHTS_CONNECTION_STRING=` (optional) | | |
| TASK-227 | Add comments to .env.example explaining how to obtain each API key | | |
| TASK-228 | Create `.gitignore` if not exists, ensure it includes: `.env`, `bin/`, `obj/`, `.vs/`, `.vscode/`, `*.user`, `secrets.json` | | |
| TASK-229 | Verify all XML documentation comments are present in API controllers for Swagger generation | | |
| TASK-230 | Run `dotnet format` across entire solution to ensure consistent code formatting | | |
| TASK-231 | Run all unit tests: `dotnet test --configuration Release --logger "console;verbosity=detailed"` - verify 100% pass | | |
| TASK-232 | Generate test coverage report: `dotnet test --collect:"XPlat Code Coverage"` - verify coverage targets met (Domain 100%, Application 90%+, Infrastructure 80%+) | | |
| TASK-233 | Build Docker image: `docker build -t weatherforecast:1.0 .` - verify successful build | | |
| TASK-234 | Run Docker container locally: `docker run -p 5000:8080 -e ConnectionStrings__Redis=host.docker.internal:6379 weatherforecast:1.0` - verify health endpoint responds | | |
| TASK-235 | Test Swagger UI: navigate to http://localhost:5000/swagger - verify all endpoints documented with examples | | |
| TASK-236 | Import Postman collection, run all requests against local Docker container - verify all tests pass | | |
| TASK-237 | Validate Bicep templates: `az bicep build --file infra/main.bicep` - verify no syntax errors | | |
| TASK-238 | Perform security scan: run `dotnet list package --vulnerable` - verify no vulnerable dependencies | | |
| TASK-239 | Add LICENSE file (MIT or Apache 2.0) in root directory | | |
| TASK-240 | Create CONTRIBUTING.md with contribution guidelines, code of conduct, pull request process | | |

### Implementation Phase 17: Azure Deployment and Validation

**GOAL-017**: Deploy infrastructure and application to Azure, validate end-to-end functionality in production

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-241 | Set up Azure service principal with OIDC for GitHub Actions: `az ad sp create-for-rbac --name weatherforecast-github --role contributor --scopes /subscriptions/{subscription-id}` | | |
| TASK-242 | Configure GitHub secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID from service principal output | | |
| TASK-243 | Configure GitHub secrets for API keys: WEATHERAPI_KEY, OPENWEATHERMAP_KEY | | |
| TASK-244 | Create Azure Resource Group: `az group create --name rg-weatherforecast-prod --location eastus` | | |
| TASK-245 | Deploy infrastructure via Bicep: `az deployment group create --resource-group rg-weatherforecast-prod --template-file infra/main.bicep --parameters infra/parameters/prod.parameters.json` | | |
| TASK-246 | Capture Bicep outputs: containerRegistryLoginServer, appInsightsConnectionString, containerAppFqdn | | |
| TASK-247 | Build Docker image with Azure Container Registry: `az acr build --registry {ACR_NAME} --image weatherforecast:latest --image weatherforecast:1.0 .` | | |
| TASK-248 | Update Container App with image and environment variables: `az containerapp update --name weatherforecast-prod --resource-group rg-weatherforecast-prod --image {ACR_LOGIN_SERVER}/weatherforecast:latest` | | |
| TASK-249 | Configure environment variables in Container App: ConnectionStrings__Redis, WeatherSources API keys, ApplicationInsights connection string | | |
| TASK-250 | Test health endpoint: `curl https://{CONTAINER_APP_FQDN}/health` - verify status Healthy | | |
| TASK-251 | Test weather forecast endpoint: `curl "https://{CONTAINER_APP_FQDN}/api/weather/forecast?date=2025-11-20&city=London&country=GB"` - verify aggregated response | | |
| TASK-252 | Import Postman collection, update Prod environment with CONTAINER_APP_FQDN, run all requests - verify all tests pass | | |
| TASK-253 | Verify OpenTelemetry data in Azure Application Insights: check request traces, dependencies, custom metrics | | |
| TASK-254 | Verify structured logs in Log Analytics: query logs with KQL, verify request/response logs present | | |
| TASK-255 | Test auto-scaling: generate load using Apache Bench or k6, verify Container App scales up to 10 replicas | | |
| TASK-256 | Test cache behavior: make same request twice, verify second request has lower response time and cacheHit=true | | |
| TASK-257 | Test graceful degradation: disable one weather source API key, verify service returns degraded health but continues serving partial data | | |
| TASK-258 | Commit all changes to Git: `git add .`, `git commit -m "feat: complete weather forecast microservice with Azure deployment"` | | |
| TASK-259 | Push to GitHub main branch: `git push origin main` - verify GitHub Actions workflow triggers and completes successfully | | |
| TASK-260 | Verify CI/CD pipeline: check workflow run in GitHub Actions, verify all jobs pass (build, test, docker push, infrastructure deploy, app deploy, post-deploy tests) | | |

## 3. Alternatives

- **ALT-001**: Hexagonal Architecture (Ports and Adapters) instead of Clean Architecture - not chosen because Clean Architecture is more explicit about layer boundaries and widely understood by .NET developers
- **ALT-002**: Single-level caching (Redis only) instead of hybrid - not chosen because L1 memory cache provides sub-millisecond performance for frequently accessed data without network latency
- **ALT-003**: Azure Kubernetes Service (AKS) instead of Azure Container Apps - not chosen because Container Apps provides simpler deployment model with built-in auto-scaling and better free tier
- **ALT-004**: Azure App Service with Docker support instead of Azure Container Apps - not chosen because Container Apps is optimized for microservices with better container-native features
- **ALT-005**: GraphQL API instead of REST - not chosen because REST is simpler for single-endpoint use case, GraphQL adds unnecessary complexity
- **ALT-006**: gRPC instead of REST - not chosen because REST provides better interoperability and easier testing with Postman
- **ALT-007**: Event-driven architecture with message queues - not chosen because synchronous request/response is sufficient for weather forecast use case
- **ALT-008**: CQRS with separate read/write models - not chosen because application has no write operations, read-only queries don't benefit from CQRS complexity
- **ALT-009**: Vertical Slice Architecture instead of Clean Architecture - not chosen because Clean Architecture provides better separation of concerns for this domain
- **ALT-010**: Terraform instead of Bicep for Infrastructure as Code - not chosen because Bicep is Azure-native with better Azure resource support and type safety
- **ALT-011**: Integration tests with TestContainers instead of unit tests only - not chosen due to time constraints and complexity, recommended as future enhancement
- **ALT-012**: Minimal APIs instead of controllers - considered but controllers provide better structure for complex validation and middleware pipeline
- **ALT-013**: URL-based API versioning (e.g., /api/v1/) instead of no versioning - not chosen for v1 as premature optimization, documented in ADR-003
- **ALT-014**: Azure SQL Database for caching instead of Redis - not chosen because Redis provides better performance for cache use case with TTL support
- **ALT-015**: Prometheus + Grafana for observability instead of OTEL + Azure Monitor - not chosen because Azure Monitor integrates seamlessly with Azure Container Apps

## 4. Dependencies

- **DEP-001**: .NET 8.0 SDK (latest LTS version) - required for compilation and runtime
- **DEP-002**: Docker Desktop (Windows or Mac) or Docker Engine (Linux) - required for containerization
- **DEP-003**: Azure CLI v2.50+ - required for infrastructure deployment and management
- **DEP-004**: Azure subscription with Contributor role - required for resource provisioning
- **DEP-005**: GitHub account with Actions enabled - required for CI/CD pipeline
- **DEP-006**: WeatherAPI.com free account and API key - required for weather data source 2
- **DEP-007**: OpenWeatherMap free account and API key - required for weather data source 3
- **DEP-008**: Visual Studio 2022 (v17.8+) or Visual Studio Code with C# extension - required for development
- **DEP-009**: Git v2.30+ - required for version control
- **DEP-010**: Postman v10+ or Newman CLI - required for API testing
- **DEP-011**: Redis 7.x (via Docker or Azure Cache) - required for distributed caching
- **DEP-012**: NuGet packages: FluentValidation (v11.9+), Polly (v8.2+), Serilog (v3.1+), AutoMapper (v12.0+), MediatR (v12.2+), StackExchange.Redis (v2.7+)
- **DEP-013**: OpenTelemetry packages: OpenTelemetry.Exporter.Console, OpenTelemetry.Exporter.OpenTelemetryProtocol, Azure.Monitor.OpenTelemetry.AspNetCore
- **DEP-014**: Test packages: xUnit (v2.6+), FluentAssertions (v6.12+), NSubstitute (v5.1+), Bogus (v35.4+)

## 5. Files

### Solution and Project Files
- **FILE-001**: `src/WeatherForecast.sln` - Solution file containing all projects
- **FILE-002**: `src/WeatherForecast.Domain/WeatherForecast.Domain.csproj` - Domain layer project
- **FILE-003**: `src/WeatherForecast.Application/WeatherForecast.Application.csproj` - Application layer project
- **FILE-004**: `src/WeatherForecast.Infrastructure/WeatherForecast.Infrastructure.csproj` - Infrastructure layer project
- **FILE-005**: `src/WeatherForecast.Api/WeatherForecast.Api.csproj` - API layer project
- **FILE-006**: `tests/WeatherForecast.Domain.Tests/WeatherForecast.Domain.Tests.csproj` - Domain unit tests
- **FILE-007**: `tests/WeatherForecast.Application.Tests/WeatherForecast.Application.Tests.csproj` - Application unit tests
- **FILE-008**: `tests/WeatherForecast.Infrastructure.Tests/WeatherForecast.Infrastructure.Tests.csproj` - Infrastructure unit tests

### Domain Layer Files
- **FILE-009**: `src/WeatherForecast.Domain/Enums/WeatherSourceType.cs` - Weather source enumeration
- **FILE-010**: `src/WeatherForecast.Domain/ValueObjects/Temperature.cs` - Temperature value object with validation
- **FILE-011**: `src/WeatherForecast.Domain/ValueObjects/Humidity.cs` - Humidity value object with validation
- **FILE-012**: `src/WeatherForecast.Domain/ValueObjects/Location.cs` - Location value object with city/country
- **FILE-013**: `src/WeatherForecast.Domain/ValueObjects/DateRange.cs` - Date range validation value object
- **FILE-014**: `src/WeatherForecast.Domain/Entities/ForecastSource.cs` - Forecast source entity
- **FILE-015**: `src/WeatherForecast.Domain/Entities/WeatherForecast.cs` - Weather forecast aggregate root
- **FILE-016**: `src/WeatherForecast.Domain/Interfaces/IWeatherRepository.cs` - Weather repository interface
- **FILE-017**: `src/WeatherForecast.Domain/Interfaces/ICacheRepository.cs` - Cache repository interface

### Application Layer Files
- **FILE-018**: `src/WeatherForecast.Application/Common/Models/WeatherForecastRequest.cs` - API request DTO
- **FILE-019**: `src/WeatherForecast.Application/Common/Models/ForecastSourceDto.cs` - Forecast source DTO
- **FILE-020**: `src/WeatherForecast.Application/Common/Models/AggregatedForecastDto.cs` - Aggregated forecast DTO
- **FILE-021**: `src/WeatherForecast.Application/Common/Models/WeatherForecastResponse.cs` - API response DTO
- **FILE-023**: `src/WeatherForecast.Application/Common/Interfaces/IWeatherSourceProvider.cs` - Weather source provider interface
- **FILE-024**: `src/WeatherForecast.Application/Common/Interfaces/IWeatherAggregationService.cs` - Aggregation service interface
- **FILE-025**: `src/WeatherForecast.Application/Common/Behaviors/ValidationBehavior.cs` - MediatR validation behavior
- **FILE-026**: `src/WeatherForecast.Application/Common/Behaviors/LoggingBehavior.cs` - MediatR logging behavior
- **FILE-027**: `src/WeatherForecast.Application/Common/Mappings/MappingProfile.cs` - AutoMapper profile
- **FILE-028**: `src/WeatherForecast.Application/Services/CacheKeyGenerator.cs` - Cache key generation service
- **FILE-029**: `src/WeatherForecast.Application/Services/WeatherAggregationService.cs` - Weather aggregation service implementation
- **FILE-030**: `src/WeatherForecast.Application/Validators/WeatherForecastRequestValidator.cs` - FluentValidation request validator
- **FILE-031**: `src/WeatherForecast.Application/DependencyInjection.cs` - Application layer DI configuration

### Infrastructure Layer Files
- **FILE-032**: `src/WeatherForecast.Infrastructure/Configuration/WeatherSourceOptions.cs` - Weather source configuration options
- **FILE-033**: `src/WeatherForecast.Infrastructure/Configuration/CacheOptions.cs` - Cache configuration options
- **FILE-034**: `src/WeatherForecast.Infrastructure/ExternalServices/OpenMeteoProvider.cs` - OpenMeteo API provider
- **FILE-035**: `src/WeatherForecast.Infrastructure/ExternalServices/WeatherApiProvider.cs` - WeatherAPI.com provider
- **FILE-036**: `src/WeatherForecast.Infrastructure/ExternalServices/OpenWeatherMapProvider.cs` - OpenWeatherMap provider
- **FILE-037**: `src/WeatherForecast.Infrastructure/ExternalServices/WeatherSourceHealthCheck.cs` - Weather source health checks
- **FILE-038**: `src/WeatherForecast.Infrastructure/Caching/HybridCacheService.cs` - Hybrid cache implementation
- **FILE-039**: `src/WeatherForecast.Infrastructure/Caching/RedisHealthCheck.cs` - Redis health check
- **FILE-040**: `src/WeatherForecast.Infrastructure/DependencyInjection.cs` - Infrastructure layer DI configuration

### API Layer Files
- **FILE-041**: `src/WeatherForecast.Api/Controllers/WeatherController.cs` - Weather API controller
- **FILE-042**: `src/WeatherForecast.Api/Middleware/ExceptionHandlingMiddleware.cs` - Global exception handler
- **FILE-043**: `src/WeatherForecast.Api/Middleware/RequestLoggingMiddleware.cs` - HTTP request/response logger
- **FILE-044**: `src/WeatherForecast.Api/Extensions/OpenTelemetryExtensions.cs` - OpenTelemetry configuration
- **FILE-045**: `src/WeatherForecast.Api/Extensions/HealthCheckExtensions.cs` - Health check endpoints configuration
- **FILE-046**: `src/WeatherForecast.Api/Program.cs` - Application entry point and configuration
- **FILE-047**: `src/WeatherForecast.Api/appsettings.json` - Application configuration (default)
- **FILE-048**: `src/WeatherForecast.Api/appsettings.Development.json` - Development environment configuration

### Test Files
- **FILE-049**: `tests/WeatherForecast.Domain.Tests/ValueObjects/TemperatureTests.cs` - Temperature value object tests
- **FILE-050**: `tests/WeatherForecast.Domain.Tests/ValueObjects/HumidityTests.cs` - Humidity value object tests
- **FILE-051**: `tests/WeatherForecast.Domain.Tests/ValueObjects/LocationTests.cs` - Location value object tests
- **FILE-052**: `tests/WeatherForecast.Domain.Tests/ValueObjects/DateRangeTests.cs` - DateRange value object tests
- **FILE-053**: `tests/WeatherForecast.Domain.Tests/Entities/WeatherForecastTests.cs` - WeatherForecast aggregate tests
- **FILE-054**: `tests/WeatherForecast.Application.Tests/Services/WeatherAggregationServiceTests.cs` - Aggregation service tests
- **FILE-055**: `tests/WeatherForecast.Application.Tests/Validators/WeatherForecastRequestValidatorTests.cs` - Validator tests
- **FILE-056**: `tests/WeatherForecast.Infrastructure.Tests/ExternalServices/OpenMeteoProviderTests.cs` - OpenMeteo provider tests
- **FILE-057**: `tests/WeatherForecast.Infrastructure.Tests/ExternalServices/WeatherApiProviderTests.cs` - WeatherAPI provider tests
- **FILE-058**: `tests/WeatherForecast.Infrastructure.Tests/ExternalServices/OpenWeatherMapProviderTests.cs` - OpenWeatherMap provider tests
- **FILE-059**: `tests/WeatherForecast.Infrastructure.Tests/Caching/HybridCacheServiceTests.cs` - Hybrid cache service tests

### Docker and Infrastructure Files
- **FILE-060**: `Dockerfile` - Multi-stage Docker build configuration
- **FILE-061**: `.dockerignore` - Docker build exclusions
- **FILE-062**: `docker-compose.yml` - Local development Docker Compose configuration
- **FILE-063**: `infra/main.bicep` - Main Bicep orchestration template
- **FILE-064**: `infra/modules/container-registry.bicep` - Azure Container Registry Bicep module
- **FILE-065**: `infra/modules/log-analytics.bicep` - Log Analytics Workspace Bicep module
- **FILE-066**: `infra/modules/app-insights.bicep` - Application Insights Bicep module
- **FILE-067**: `infra/modules/redis.bicep` - Azure Cache for Redis Bicep module
- **FILE-068**: `infra/modules/container-app.bicep` - Azure Container App Bicep module
- **FILE-069**: `infra/parameters/dev.parameters.json` - Development environment parameters
- **FILE-070**: `infra/parameters/prod.parameters.json` - Production environment parameters

### CI/CD and Documentation Files
- **FILE-071**: `.github/workflows/deploy.yml` - GitHub Actions CI/CD workflow
- **FILE-072**: `README.md` - Project documentation and setup guide
- **FILE-073**: `docs/adr/ADR-001-clean-architecture.md` - ADR for Clean Architecture decision
- **FILE-074**: `docs/adr/ADR-002-hybrid-caching.md` - ADR for caching strategy decision
- **FILE-075**: `docs/adr/ADR-003-no-api-versioning.md` - ADR for no API versioning decision
- **FILE-076**: `docs/adr/ADR-004-azure-container-apps.md` - ADR for Azure Container Apps decision
- **FILE-077**: `docs/adr/ADR-005-weather-api-sources.md` - ADR for weather API source selection
- **FILE-078**: `.env.example` - Environment variables template
- **FILE-079**: `.gitignore` - Git exclusions
- **FILE-080**: `LICENSE` - Project license file
- **FILE-081**: `CONTRIBUTING.md` - Contribution guidelines

### Postman Collection Files
- **FILE-082**: `postman/WeatherForecast.postman_collection.json` - Postman collection with all API requests and tests
- **FILE-083**: `postman/WeatherForecast-Dev.postman_environment.json` - Development environment variables
- **FILE-084**: `postman/WeatherForecast-Prod.postman_environment.json` - Production environment variables

## 6. Testing

### Domain Layer Tests
- **TEST-001**: Temperature value object validation - test valid range (-100 to 60°C), invalid values throw ArgumentException
- **TEST-002**: Temperature equality comparison - test two Temperature objects with same value are equal
- **TEST-003**: Humidity value object validation - test valid range (0-100%), invalid values throw ArgumentException
- **TEST-004**: Humidity equality comparison - test two Humidity objects with same value are equal
- **TEST-005**: Location value object validation - test non-empty city/country, valid ISO 3166-1 alpha-2 country code
- **TEST-006**: Location equality comparison - test two Location objects with same city/country are equal
- **TEST-007**: DateRange value object validation - test dates within ±7 days pass, dates beyond throw ArgumentException
- **TEST-008**: WeatherForecast aggregate AddSource method - test adding forecast sources to aggregate
- **TEST-009**: WeatherForecast aggregate CalculateAverage - test average calculation with all sources available
- **TEST-010**: WeatherForecast aggregate CalculateAverage - test average calculation with partial sources (1-2 unavailable)

### Application Layer Tests
- **TEST-011**: WeatherAggregationService cache hit - mock cache returns value, verify providers not called
- **TEST-012**: WeatherAggregationService cache miss - mock cache returns null, verify all providers called in parallel
- **TEST-013**: WeatherAggregationService successful aggregation - all sources return data, verify correct average calculated
- **TEST-014**: WeatherAggregationService partial failure - 1 source fails, verify aggregation includes only available sources
- **TEST-015**: WeatherAggregationService all sources fail - verify appropriate response with all sources marked unavailable
- **TEST-016**: WeatherAggregationService source filtering - sources parameter provided, verify only specified providers called
- **TEST-017**: WeatherAggregationService caching - verify result cached after successful aggregation
- **TEST-018**: WeatherForecastRequestValidator valid request - date within range, required fields present, passes validation
- **TEST-019**: WeatherForecastRequestValidator invalid date range - date >7 days future/past, fails validation
- **TEST-020**: WeatherForecastRequestValidator missing required fields - city or country missing, fails validation
- **TEST-021**: WeatherForecastRequestValidator invalid country code - non-ISO format, fails validation
- **TEST-022**: CacheKeyGenerator - verify correct format: weather:{city}:{country}:{yyyyMMdd}:{sources}

### Infrastructure Layer Tests
- **TEST-023**: OpenMeteoProvider successful API call - mock HTTP response, verify ForecastSource populated correctly, Available=true
- **TEST-024**: OpenMeteoProvider HTTP failure - mock throws HttpRequestException, verify Available=false, Error message populated
- **TEST-025**: OpenMeteoProvider timeout - mock delays >5s, verify timeout handled, Available=false
- **TEST-026**: OpenMeteoProvider circuit breaker - verify circuit opens after 3 consecutive failures
- **TEST-027**: WeatherApiProvider successful API call - mock HTTP response with API key, verify data parsed correctly
- **TEST-028**: WeatherApiProvider invalid API key - mock 401 response, verify error handled gracefully
- **TEST-029**: OpenWeatherMapProvider successful API call - mock HTTP response, verify temperature/humidity extracted
- **TEST-030**: OpenWeatherMapProvider rate limit exceeded - mock 429 response, verify retry mechanism
- **TEST-031**: HybridCacheService GetAsync L1 hit - verify L2 (Redis) not called when L1 cache has value
- **TEST-032**: HybridCacheService GetAsync L1 miss, L2 hit - verify L1 populated with value from L2
- **TEST-033**: HybridCacheService GetAsync complete miss - verify null returned when both L1 and L2 miss
- **TEST-034**: HybridCacheService SetAsync - verify value written to both L1 and L2 with correct TTL
- **TEST-035**: HybridCacheService SetAsync Redis disabled - verify only L1 called when EnableDistributedCache=false
- **TEST-036**: HybridCacheService Redis unavailable - verify fallback to L1 only, no exceptions thrown
- **TEST-037**: HybridCacheService sliding expiration - verify Redis TTL refreshed on cache hit when SlidingExpiration=true
- **TEST-038**: WeatherSourceHealthCheck all sources healthy - verify Healthy status returned
- **TEST-039**: WeatherSourceHealthCheck partial degradation - 1-2 sources down, verify Degraded status returned
- **TEST-040**: WeatherSourceHealthCheck all sources unhealthy - verify Unhealthy status returned
- **TEST-041**: RedisHealthCheck Redis available - verify Healthy status when Redis ping succeeds
- **TEST-042**: RedisHealthCheck Redis unavailable - verify Unhealthy status when Redis ping fails

## 7. Risks & Assumptions

### Risks
- **RISK-001**: **Weather API rate limits exceeded** - If service becomes popular, free tier limits may be exceeded (OpenMeteo: 10k/day, WeatherAPI: 1M/month, OpenWeatherMap: 1k/day). **Mitigation**: Implement caching aggressively (1 hour TTL), monitor API usage, consider upgrading to paid tiers if needed
- **RISK-002**: **Weather API service availability** - External APIs may experience downtime or breaking changes. **Mitigation**: Graceful degradation design, health checks with degraded status, multiple redundant sources
- **RISK-003**: **Azure free tier exhaustion** - Container Apps free tier (180k vCPU-seconds/month) may be insufficient for production load. **Mitigation**: Monitor usage, implement aggressive auto-scaling down to 0 replicas, upgrade to paid tier if needed
- **RISK-004**: **Cache consistency issues** - Cached data may become stale if weather conditions change rapidly. **Mitigation**: Appropriate TTL configuration (15 min memory, 60 min Redis), document cache behavior in API docs
- **RISK-005**: **Redis connection failures** - Redis unavailability could impact cache performance. **Mitigation**: Hybrid cache with fallback to memory-only mode, Redis health check
- **RISK-006**: **Geocoding accuracy** - Converting city names to coordinates may be ambiguous (e.g., "Springfield" exists in multiple countries). **Mitigation**: Require country parameter, use first match from geocoding API, document limitation
- **RISK-007**: **API key exposure** - Accidental commit of API keys to source control. **Mitigation**: Use .gitignore for .env files, Azure Key Vault for production, security scanning in CI pipeline
- **RISK-008**: **Breaking changes in weather APIs** - External APIs may change response format without notice. **Mitigation**: Comprehensive error handling, logging, monitoring, quick rollback capability
- **RISK-009**: **Container image vulnerabilities** - Base images may contain security vulnerabilities. **Mitigation**: Regular image updates, Trivy scanning in CI pipeline, use official Microsoft images
- **RISK-010**: **Insufficient test coverage** - Unit tests only (no integration tests) may miss integration issues. **Mitigation**: Comprehensive unit tests (80%+ coverage), manual testing, post-deployment smoke tests, future: add integration tests
- **RISK-011**: **CI/CD pipeline failure** - GitHub Actions workflow failures could block deployments. **Mitigation**: Comprehensive testing before push, rollback capability, manual deployment as fallback
- **RISK-012**: **Azure resource provisioning delays** - Bicep deployment may take 10-15 minutes. **Mitigation**: Separate infrastructure deployment from app deployment, use existing infrastructure when possible
- **RISK-013**: **Weather data accuracy** - Different sources may return significantly different values. **Mitigation**: Aggregation logic averages values, return all sources in response for transparency, document data sources in API docs

### Assumptions
- **ASSUMPTION-001**: Weather forecast data from different sources can be meaningfully averaged
- **ASSUMPTION-002**: Free tier limits of weather APIs are sufficient for development and testing
- **ASSUMPTION-003**: 7-day forecast range is acceptable for user requirements (most free APIs support this)
- **ASSUMPTION-004**: City name + country code uniquely identifies a location (may have ambiguity)
- **ASSUMPTION-005**: Temperature in Celsius and humidity in percentage are standard units for all sources
- **ASSUMPTION-006**: Azure free tier resources are sufficient for MVP deployment and demo purposes
- **ASSUMPTION-007**: Redis cache is not critical for service operation (can fallback to memory-only)
- **ASSUMPTION-008**: No authentication/authorization required for v1 (public API)
- **ASSUMPTION-009**: No rate limiting required beyond what Azure Container Apps provides
- **ASSUMPTION-010**: CORS not required for v1 (backend-only service, no browser clients)
- **ASSUMPTION-011**: Clean Architecture overhead is justified by maintainability benefits
- **ASSUMPTION-012**: Unit tests provide sufficient confidence (integration tests deferred to v2)
- **ASSUMPTION-013**: 1-hour cache TTL is acceptable for weather data freshness
- **ASSUMPTION-014**: OpenTelemetry data export to Azure Monitor is sufficient for observability
- **ASSUMPTION-015**: GitHub Actions is available and reliable for CI/CD
- **ASSUMPTION-016**: Development team has access to Azure subscription with Contributor role
- **ASSUMPTION-017**: Development team has Visual Studio 2022 or VS Code with C# extension
- **ASSUMPTION-018**: Postman collection is sufficient for API testing (no custom test framework needed)
- **ASSUMPTION-019**: English language is acceptable for all documentation and error messages
- **ASSUMPTION-020**: No data persistence required beyond caching (stateless service)

## 8. Related Specifications / Further Reading

### Microsoft Documentation
- [Clean Architecture in ASP.NET Core](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Azure Container Apps Documentation](https://docs.microsoft.com/en-us/azure/container-apps/)
- [Azure Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [OpenTelemetry in .NET](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing)
- [Serilog Structured Logging](https://github.com/serilog/serilog/wiki)

### Design Patterns and Architecture
- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/reference/)
- [Martin Fowler - Patterns of Enterprise Application Architecture](https://martinfowler.com/eaaCatalog/)
- [Clean Code by Robert C. Martin](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

### Weather API Documentation
- [OpenMeteo API Documentation](https://open-meteo.com/en/docs)
- [WeatherAPI.com Documentation](https://www.weatherapi.com/docs/)
- [OpenWeatherMap API Documentation](https://openweathermap.org/api)

### Resilience and Caching
- [Polly Resilience Framework](https://github.com/App-vNext/Polly)
- [Redis Caching Best Practices](https://redis.io/docs/manual/patterns/)
- [Two-Level Cache Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside)

### Testing
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

### CI/CD and DevOps
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Azure DevOps Deployment Patterns](https://docs.microsoft.com/en-us/azure/architecture/framework/devops/deployment)

### Related LockerOps Patterns
- LockerOps Copilot Playbook (internal reference)
- LockerOps Architecture Patterns (internal reference)
- LockerOps Code Review Guidelines (internal reference)

---

**Implementation Plan Complete**

This comprehensive implementation plan provides 260 atomic, executable tasks across 17 phases to deliver a production-grade dockerized .NET 8 weather forecast aggregation microservice with Clean Architecture, comprehensive testing, observability, and automated Azure deployment. Each task is specific, measurable, and can be executed by AI agents or developers without ambiguity.

**Next Steps:**
1. Review and approve this implementation plan
2. Begin Phase 1 execution (Project Foundation & Structure)
3. Track progress by marking tasks as completed with dates
4. Update status badge to "In Progress" when execution begins
5. Create GitHub Issues for any identified risks or technical debt

**Estimated Total Implementation Time:** 7-10 days for single developer, 3-5 days for team of 2-3 developers working in parallel on independent phases.