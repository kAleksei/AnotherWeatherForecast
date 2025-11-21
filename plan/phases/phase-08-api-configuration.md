---
phase: 8
title: API Layer - Program.cs and Configuration Files
goal: Configure application startup, dependency injection, middleware pipeline, and settings
status: Planned
---

# Implementation Phase 8: API Layer - Program.cs and Configuration Files

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
| TASK-094 | Configure WeatherSources section: OpenMeteo (BaseUrl, Enabled, TimeoutSeconds), WeatherAPI (BaseUrl, ApiKey, Enabled, TimeoutSeconds), OpenWeatherMap (BaseUrl, ApiKey, Enabled, TimeoutSeconds) | | |
| TASK-095 | Configure CacheSettings section: MemoryCacheDurationMinutes=15, RedisCacheDurationMinutes=60, EnableDistributedCache=true, SlidingExpiration=true | | |
| TASK-096 | Configure ConnectionStrings section: Redis="localhost:6379" | | |
| TASK-097 | Configure Serilog section: MinimumLevel (Default=Information), WriteTo (Console with CompactJsonFormatter), Enrich (FromLogContext, WithMachineName) | | |
| TASK-098 | Create `appsettings.Development.json` overriding: ConnectionStrings:Redis, ApplicationInsights disabled | | |
| TASK-099 | Configure project properties: enable XML documentation generation for Swagger, set Docker support | | |

