---
phase: 5
title: Infrastructure Layer - External API Providers
goal: Implement weather source providers with HTTP clients, resilience policies, and error handling
status: Planned
---

# Implementation Phase 5: Infrastructure Layer - External API Providers

### Implementation Phase 5: Infrastructure Layer - External API Providers

**GOAL-005**: Implement weather source providers with HTTP clients, resilience policies, and error handling

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-041 | Install NuGet packages in Infrastructure: `Microsoft.Extensions.Http.Polly`, `StackExchange.Redis`, `Microsoft.Extensions.Caching.Memory`, `Polly` | | |
| TASK-042 | Create `WeatherSourceOptions` in `src/WeatherForecast.Infrastructure/Configuration/WeatherSourceOptions.cs` with BaseUrl, ApiKey, Enabled, TimeoutSeconds properties | | |
| TASK-043 | Create `OpenMeteoProvider` in `src/WeatherForecast.Infrastructure/ExternalServices/OpenMeteoProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-044 | Implement OpenMeteo API integration: endpoint `https://api.open-meteo.com/v1/forecast`, parameters: latitude, longitude (from geocoding), date, temperature_2m, relative_humidity_2m | | |
| TASK-045 | Create `WeatherApiProvider` in `src/WeatherForecast.Infrastructure/ExternalServices/WeatherApiProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-046 | Implement WeatherAPI.com integration: endpoint `https://api.weatherapi.com/v1/{forecast or history}.json`, API key authentication, parse JSON response | | |
| TASK-047 | Create `OpenWeatherMapProvider` in `src/WeatherForecast.Infrastructure/ExternalServices/OpenWeatherMapProvider.cs` implementing `IWeatherSourceProvider` | | |
| TASK-048 | Implement OpenWeatherMap integration: endpoint `https://api.openweathermap.org/data/2.5/forecast` or `onecall/timemachine`, API key authentication | | |
| TASK-049 | Configure Polly policies for all HTTP clients: Retry (3 attempts, exponential backoff), Circuit Breaker (3 failures, 30s break), Timeout (5s) | | |
| TASK-050 | Implement error handling in each provider: catch HttpRequestException, TimeoutException, map to ForecastSource with Available=false and Error message | | |
| TASK-051 | Add structured logging in each provider: log request start, success, failure with correlation IDs | | |

