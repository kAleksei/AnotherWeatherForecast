---
phase: 10
title: Unit Tests - Infrastructure Layer
goal: Implement unit tests for weather providers, caching service, and health checks
status: Planned
---

# Implementation Phase 10: Unit Tests - Infrastructure Layer

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

