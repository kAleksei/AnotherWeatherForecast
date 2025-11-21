---
phase: 6
title: Infrastructure Layer - Caching and Health Checks
goal: Implement two-level caching (memory + Redis) and comprehensive health checks with degradation support
status: Planned
---

# Implementation Phase 6: Infrastructure Layer - Caching and Health Checks

### Implementation Phase 6: Infrastructure Layer - Caching and Health Checks

**GOAL-006**: Implement two-level caching (memory + Redis) and comprehensive health checks with degradation support

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-052 | Create `CacheOptions` in `src/WeatherForecast.Infrastructure/Configuration/CacheOptions.cs` with MemoryCacheDuration, RedisCacheDuration, EnableDistributedCache, SlidingExpiration properties | | |
| TASK-053 | Create `HybridCacheService` in `src/WeatherForecast.Infrastructure/Caching/HybridCacheService.cs` implementing `ICacheRepository` | | |
| TASK-054 | Implement L1 (memory) cache in `HybridCacheService`: use `IMemoryCache`, check memory first, TTL from config | | |
| TASK-055 | Implement L2 (Redis) cache in `HybridCacheService`: use `IConnectionMultiplexer`, fallback to memory if Redis unavailable, TTL from config | | |
| TASK-056 | Implement sliding expiration for Redis cache when `SlidingExpiration=true` in configuration | | |
| TASK-057 | Implement `GetAsync<T>`: check L1 â†’ if miss check L2 â†’ if hit, populate L1 â†’ return value | | |
| TASK-058 | Implement `SetAsync<T>`: write to both L1 and L2 (if enabled), serialize to JSON for Redis | | |
| TASK-059 | Create `WeatherSourceHealthCheck` in `src/WeatherForecast.Infrastructure/ExternalServices/WeatherSourceHealthCheck.cs` implementing `IHealthCheck` | | |
| TASK-060 | Implement parallel health checks for all weather sources: test HTTP connectivity, API key validity | | |
| TASK-061 | Implement health check logic: Healthy (all sources OK), Degraded (1+ sources down), Unhealthy (all sources down) | | |
| TASK-062 | Create `RedisHealthCheck` in `src/WeatherForecast.Infrastructure/Caching/RedisHealthCheck.cs`: ping Redis, return Healthy/Unhealthy | | |
| TASK-063 | Create `DependencyInjection.cs` in `src/WeatherForecast.Infrastructure/DependencyInjection.cs` with `AddInfrastructure(IServiceCollection, IConfiguration)` extension method | | |
| TASK-064 | Register weather providers as singletons with named HttpClients in `DependencyInjection.cs` | | |
| TASK-065 | Register Redis connection: `services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString))` | | |
| TASK-066 | Register memory cache: `services.AddMemoryCache(options => options.SizeLimit = configSizeMB)` | | |
| TASK-067 | Register health checks: `services.AddHealthChecks().AddCheck<WeatherSourceHealthCheck>().AddCheck<RedisHealthCheck>()` | | |

