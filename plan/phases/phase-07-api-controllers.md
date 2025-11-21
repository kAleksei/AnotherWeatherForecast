---
phase: 7
title: API Layer - Controllers, Middleware, and Configuration
goal: Implement REST API endpoint, exception handling middleware, and API configuration
status: Planned
---

# Implementation Phase 7: API Layer - Controllers, Middleware, and Configuration

### Implementation Phase 7: API Layer - Controllers, Middleware, and Configuration

**GOAL-007**: Implement REST API endpoint, exception handling middleware, and API configuration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-068 | Install NuGet packages in API project: `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Formatting.Compact`, `OpenTelemetry.Exporter.Console`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `Azure.Monitor.OpenTelemetry.AspNetCore` | | |
| TASK-069 | Create `WeatherController` in `src/WeatherForecast.Api/Controllers/WeatherController.cs` inheriting `ControllerBase` with `[ApiController]` and `[Route("api/weather")]` | | |
| TASK-070 | Implement `GetForecast` action: `[HttpGet("forecast")]` accepting query parameters: date, city, country, sources (optional) | | |
| TASK-071 | Inject `IWeatherAggregationService` and `IValidator<WeatherForecastRequest>` into controller constructor | | |
| TASK-072 | Implement validation in action: call validator, return `BadRequest(errors)` if validation fails | | |
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

