# AnotherWeatherForecast

A Weather Forecast Aggregation Microservice built with .NET 8 following Clean Architecture and Domain-Driven Design (DDD) principles.

## Overview

This service aggregates weather forecast data from multiple free API sources (OpenMeteo, WeatherAPI.com, OpenWeatherMap) and provides a REST API with caching, observability, and resilience patterns.

## Architecture

The solution follows **Clean Architecture** with four distinct layers:

1. **Domain Layer** (`WeatherForecast.Domain`) - Core business logic, entities, value objects, and interfaces
2. **Application Layer** (`WeatherForecast.Application`) - Business logic orchestration, DTOs, validation, and service interfaces
3. **Infrastructure Layer** (`WeatherForecast.Infrastructure`) - External dependencies, API clients, caching, and health checks
4. **API Layer** (`WeatherForecast.Api`) - REST API endpoints, middleware, and configuration

## Prerequisites

- .NET 8.0 SDK or higher
- Docker Desktop (for containerized development)
This service relies on the in-memory cache provided by `IMemoryCache` and does not depend on Redis.

## Build and Test

### Build the Solution

```bash
# Restore dependencies
dotnet restore AnotherWeatherForecast.slnx

# Build in Release configuration
dotnet build AnotherWeatherForecast.slnx --configuration Release
```

### Run Tests

```bash
# Run all tests
dotnet test AnotherWeatherForecast.slnx

# Run tests with detailed output
dotnet test AnotherWeatherForecast.slnx --logger "console;verbosity=detailed"
```

### Run Locally

```bash
# Run API project directly
cd src/WeatherForecast.Api
dotnet run

# Run with Docker Compose
docker-compose up
```

## Postman Collection & Tests

Use the included Postman collection and environment templates to run end-to-end checks.

```bash
newman run postman/WeatherForecast.postman_collection.json \
	-e postman/WeatherForecast-Dev.postman_environment.json
```

## Continuous Integration

This repository uses GitHub Actions to ensure code quality and build integrity. The CI workflow automatically runs on every pull request and push to the `main` or `develop` branches.

### CI Workflow

The CI workflow performs the following checks:

1. **Checkout code** - Retrieves the latest code from the repository
2. **Setup .NET 8 SDK** - Installs the .NET 8 SDK
3. **Restore dependencies** - Restores NuGet packages
4. **Build solution** - Builds the entire solution in Release configuration
5. **Run tests** - Executes all unit tests

### Pull Request Requirements

**All pull requests must pass the CI build before they can be merged.** This ensures that:

- The code builds successfully
- All unit tests pass
- No breaking changes are introduced
- The solution remains in a deployable state

### CI Status

You can view the status of CI builds in the following places:

- **Pull Request page** - The CI status is displayed as a check in the PR conversation
- **Actions tab** - View detailed logs and history of all CI runs
- **Commit status** - Each commit shows whether the build passed or failed

### Local Pre-commit Checks

Before pushing your changes, ensure your code builds and tests pass locally:

```bash
# Quick validation
dotnet build AnotherWeatherForecast.slnx --configuration Release
dotnet test AnotherWeatherForecast.slnx --configuration Release --no-build
```

This helps catch issues early and speeds up the code review process.

## Contributing

1. Fork the repository
2. Create a feature branch from `develop`
3. Make your changes and ensure tests pass locally
4. Push your changes and create a pull request
5. Wait for CI to pass and address any feedback
6. Once approved, your changes will be merged

## Documentation

For detailed architectural guidelines and development practices, see:

- [GitHub Copilot Instructions](/.github/copilot-instructions.md) - Development guidelines and conventions

## License

[Add your license information here]
