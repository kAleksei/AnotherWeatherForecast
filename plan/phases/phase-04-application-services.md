---
phase: 4
title: Application Layer - Services and Behaviors
goal: Implement core business logic for weather aggregation, caching, and cross-cutting concerns
status: Planned
---

# Implementation Phase 4: Application Layer - Services and Behaviors

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

