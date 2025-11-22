---
phase: 6
title: Infrastructure Layer - Caching with Decorator Pattern
goal: Implement caching at provider level using CachedWeatherSourceProvider decorator with HybridCache
status: Planned
---

# Implementation Phase 6: Infrastructure Layer - Caching with Decorator Pattern

### Implementation Phase 6: Infrastructure Layer - Caching with Decorator Pattern

**GOAL-006**: Implement caching at provider level using CachedWeatherSourceProvider decorator with HybridCache

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-052 | Add `Microsoft.Extensions.Caching.Hybrid` NuGet package (v10.0+) to Infrastructure project | | |
| TASK-053 | Create `CachedWeatherSourceProvider` decorator in `src/WeatherForecast.Infrastructure/Caching/CachedWeatherSourceProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-054 | Implement caching logic using HybridCache.GetOrCreateAsync with per-provider TTL from `WeatherSourceOptions.CacheDurationMinutes` | | |
| TASK-055 | Generate cache keys with format: `weather:{SourceName}:{City}:{Country}:{yyyyMMdd}` | | |
| TASK-056 | Register HybridCache in DependencyInjection: `services.AddHybridCache()` | | |
| TASK-057 | Wrap each weather provider with `CachedWeatherSourceProvider` in DI registration | | |
| TASK-058 | Remove `ICacheRepository` dependency from `WeatherAggregationService` and delete caching logic | | |
