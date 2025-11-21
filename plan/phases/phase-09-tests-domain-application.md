---
phase: 9
title: Unit Tests - Domain and Application Layers
goal: Implement comprehensive unit tests for domain entities, value objects, services, and validators
status: Planned
---

# Implementation Phase 9: Unit Tests - Domain and Application Layers

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
| TASK-108 | Test DateRange value object: Â±7 days validation, dates beyond range throw exception | | |
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

