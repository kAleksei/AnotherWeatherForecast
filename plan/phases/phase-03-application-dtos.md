---
phase: 3
title: Application Layer - DTOs, Interfaces, and Validation
goal: Define application contracts, DTOs, validation rules, and service interfaces
status: Completed
completed_date: 2025-11-21
---

# Implementation Phase 3: Application Layer - DTOs, Interfaces, and Validation

### Implementation Phase 3: Application Layer - DTOs, Interfaces, and Validation

**GOAL-003**: Define application contracts, DTOs, validation rules, and service interfaces

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Create `WeatherForecastRequest` DTO in `src/WeatherForecast.Application/Common/Models/WeatherForecastRequest.cs` with Date, City, Country, Sources (optional) properties | ✅ | 2025-11-21 |
| TASK-024 | Create `ForecastSourceDto` DTO in `src/WeatherForecast.Application/Common/Models/ForecastSourceDto.cs` matching API response schema | ✅ | 2025-11-21 |
| TASK-025 | Create `AggregatedForecastDto` DTO in `src/WeatherForecast.Application/Common/Models/AggregatedForecastDto.cs` with AverageTemperature, AverageHumidity, TemperatureRange | ✅ | 2025-11-21 |
| TASK-026 | Create `WeatherForecastResponse` DTO in `src/WeatherForecast.Application/Common/Models/WeatherForecastResponse.cs` with Location, Date, AggregatedForecast, Sources | ✅ | 2025-11-21 |
| TASK-027 | Create `ResponseMetadata` DTO in `src/WeatherForecast.Application/Common/Models/ResponseMetadata.cs` with properties: RequestId, Timestamp, Status, Message | ✅ | 2025-11-21 |
| TASK-028 | Create `IWeatherSourceProvider` interface in `src/WeatherForecast.Application/Common/Interfaces/IWeatherSourceProvider.cs` with methods: `Task<ForecastSource> GetForecastAsync`, `string SourceName`, `bool IsEnabled` | ✅ | 2025-11-21 |
| TASK-029 | Create `IWeatherAggregationService` interface in `src/WeatherForecast.Application/Common/Interfaces/IWeatherAggregationService.cs` with method: `Task<WeatherForecastResponse> GetAggregatedForecastAsync` | ✅ | 2025-11-21 |
| TASK-030 | Create `WeatherForecastRequestValidator` in `src/WeatherForecast.Application/Validators/WeatherForecastRequestValidator.cs` using FluentValidation: validate single date (+7 days from today and no limitation to past), required fields, country code format | ✅ | 2025-11-21 |
| TASK-031 | Install NuGet packages in Application project: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, ~~`AutoMapper`~~, `MediatR` | ✅ | 2025-11-21 |
| TASK-032 | Create manual mapping extensions in `src/WeatherForecast.Application/Common/Extensions/MappingExtensions.cs` for Entity → DTO mappings (replaced AutoMapper with extension methods) | ✅ | 2025-11-21 |


