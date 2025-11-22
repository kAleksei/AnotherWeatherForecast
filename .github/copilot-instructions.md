# GitHub Copilot Instructions for AnotherWeatherForecast

## Project Overview

This is a **Weather Forecast Aggregation Microservice** built with **.NET 8** following **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The service aggregates weather forecast data from multiple free API sources (OpenMeteo, WeatherAPI.com, OpenWeatherMap) and provides a REST API with caching, observability, and resilience patterns.

### Architecture

The solution follows **Clean Architecture** with four distinct layers:

1. **Domain Layer** (`WeatherForecast.Domain`) - Core business logic, entities, value objects, and interfaces
2. **Application Layer** (`WeatherForecast.Application`) - Business logic orchestration, DTOs, validation, and service interfaces
3. **Infrastructure Layer** (`WeatherForecast.Infrastructure`) - External dependencies, API clients, caching, and health checks
4. **API Layer** (`WeatherForecast.Api`) - REST API endpoints, middleware, and configuration

### Key Design Patterns

- **Clean Architecture**: Separation of concerns with dependency inversion
- **Domain-Driven Design**: Value objects, entities, aggregates
- **Repository Pattern**: Abstraction for data access
- **Strategy Pattern**: Multiple weather source implementations
- **CQRS Pattern** (optional): Command/Query separation via MediatR
- **Options Pattern**: Strongly-typed configuration with validation
- **Decorator Pattern**: Resilience policies with Polly

## Build, Test, and Run Commands

### Prerequisites

- .NET 8.0 SDK or higher
- Docker Desktop (for containerized development)
- Redis (optional, can run with memory-only cache)

### Build

```bash
# Build entire solution
dotnet build AnotherWeatherForecast.slnx

# Build specific project
dotnet build src/WeatherForecast.Api/WeatherForecast.Api.csproj

# Build in Release mode
dotnet build AnotherWeatherForecast.slnx --configuration Release
```

### Test

```bash
# Run all tests
dotnet test AnotherWeatherForecast.slnx

# Run tests with detailed output
dotnet test AnotherWeatherForecast.slnx --logger "console;verbosity=detailed"

# Run tests with coverage
dotnet test AnotherWeatherForecast.slnx --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/AnotherWeatherForecast.Domain.Tests/AnotherWeatherForecast.Domain.Tests.csproj
```

### Run Locally

```bash
# Run API project directly
cd src/WeatherForecast.Api
dotnet run

# Run with specific environment
dotnet run --environment Development

# Run with Docker Compose (includes Redis)
docker-compose up
```

### Restore Dependencies

```bash
# Restore NuGet packages for solution
dotnet restore AnotherWeatherForecast.slnx
```

### Clean

```bash
# Clean build artifacts
dotnet clean AnotherWeatherForecast.slnx
```

## Development Workflow

### Making Changes

1. **Domain Layer Changes**: Start here if changing business logic, entities, or value objects
2. **Application Layer Changes**: Modify DTOs, services, or validation rules
3. **Infrastructure Layer Changes**: Update external API integrations, caching, or persistence
4. **API Layer Changes**: Modify controllers, middleware, or configuration

### Before Committing

Always ensure:
1. Code builds successfully: `dotnet build AnotherWeatherForecast.slnx`
2. All tests pass: `dotnet test AnotherWeatherForecast.slnx`
3. Code follows existing conventions (see below)

### Testing Strategy

- **Unit Tests Only**: This project uses unit tests exclusively (no integration tests currently)
- **Test Coverage Targets**:
  - Domain Layer: 100%
  - Application Layer: 90%+
  - Infrastructure Layer: 80%+
- **Testing Tools**: xUnit, FluentAssertions, NSubstitute (mocking), Bogus (test data generation)

## Code Style and Conventions

### General Guidelines

- **Follow Clean Code principles**: Readable, maintainable, self-documenting code
- **SOLID Principles**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Naming Conventions**:
  - Use meaningful, descriptive names (no abbreviations)
  - PascalCase for classes, methods, properties, public members
  - camelCase for local variables and parameters
  - Prefix interfaces with `I` (e.g., `IWeatherRepository`)
  - Suffix DTOs with `Dto` (e.g., `WeatherForecastDto`)
  - Suffix validators with `Validator` (e.g., `WeatherForecastRequestValidator`)

### C# Conventions

- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- **Target Framework**: .NET 8.0 (`net8.0`)
- **Use `var`** when the type is obvious from the right side
- **Use explicit types** when clarity is needed
- **Async/Await**: Always use `async`/`await` for I/O operations
- **Cancellation Tokens**: Pass `CancellationToken` for async operations

### Architecture Patterns to Follow

#### Domain Layer

- **No external dependencies** (except standard .NET libraries)
- **Value Objects**: Immutable, equality by value, validation in constructor
- **Entities**: Identity-based equality, encapsulated business logic
- **Interfaces only**: Define contracts, implementations go in Infrastructure
- **Example Value Object**:
  ```csharp
  public sealed class Temperature : IEquatable<Temperature>
  {
      public decimal Celsius { get; }
      
      public Temperature(decimal celsius)
      {
          if (celsius < -100 || celsius > 60)
              throw new ArgumentOutOfRangeException(nameof(celsius));
          Celsius = celsius;
      }
      
      public bool Equals(Temperature? other) => 
          other is not null && Celsius == other.Celsius;
  }
  ```

#### Application Layer

- **DTOs**: Simple data containers, no business logic
- **Service Interfaces**: Define contracts for business operations
- **Validators**: Use FluentValidation for input validation
- **Dependency Injection**: Register services in `DependencyInjection.cs`
- **Example Service**:
  ```csharp
  public class WeatherAggregationService : IWeatherAggregationService
  {
      private readonly IEnumerable<IWeatherSourceProvider> _providers;
      private readonly ICacheRepository _cache;
      
      public async Task<WeatherForecastResponse> GetAggregatedForecastAsync(
          WeatherForecastRequest request, 
          CancellationToken cancellationToken)
      {
          // Implementation
      }
  }
  ```

#### Infrastructure Layer

- **External dependencies**: HTTP clients, database, caching, third-party APIs
- **Resilience**: Use Polly for retry, circuit breaker, timeout policies
- **Configuration**: Use Options pattern with strongly-typed classes
- **Health Checks**: Implement `IHealthCheck` for each external dependency
- **Example Provider**:
  ```csharp
  public class OpenMeteoProvider : IWeatherSourceProvider
  {
      private readonly HttpClient _httpClient;
      private readonly ILogger<OpenMeteoProvider> _logger;
      
      public async Task<ForecastSource> GetForecastAsync(
          Location location, 
          DateTime date, 
          CancellationToken cancellationToken)
      {
          try
          {
              // API call with error handling
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Failed to get forecast");
              return new ForecastSource { Available = false, Error = ex.Message };
          }
      }
  }
  ```

#### API Layer

- **Controllers**: Thin controllers, delegate to services
- **Middleware**: Global exception handling, request logging
- **Configuration**: Use `appsettings.json` with environment-specific overrides
- **OpenAPI/Swagger**: Document all endpoints with XML comments
- **Example Controller**:
  ```csharp
  [ApiController]
  [Route("api/[controller]")]
  /// <summary>
  /// Controller for aggregating weather forecasts from multiple sources.
  /// </summary>
  public class WeatherController : ControllerBase
  {
      /// <summary>
      /// Gets aggregated weather forecast from multiple sources
      /// </summary>
      /// <param name="request">Weather forecast request parameters</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Aggregated weather forecast response</returns>
      [HttpGet("forecast")]
      [ProducesResponseType(typeof(WeatherForecastResponse), StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> GetForecast(
          [FromQuery] WeatherForecastRequest request,
          CancellationToken cancellationToken)
      {
          // Implementation
      }
  }
  ```

### Error Handling

- **Use exceptions** for exceptional cases only
- **Return error objects** for expected failures (e.g., validation errors)
- **Log errors** with structured logging (Serilog)
- **Don't expose internal details** in API responses (use ProblemDetails)
- **Graceful degradation**: Service should continue operating with partial data

### Dependency Injection

Each layer has a `DependencyInjection.cs` file with an extension method:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register Application layer services
        services.AddScoped<IWeatherAggregationService, WeatherAggregationService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}
```

### Configuration

- **Use Options Pattern**: Strongly-typed configuration classes
- **Validate on startup**: Use `ValidateDataAnnotations()` or FluentValidation
- **Environment variables**: Override `appsettings.json` values in production
- **Secrets**: Use Azure Key Vault in production, user secrets in development
- **Example**:
  ```csharp
  public class WeatherSourceOptions
  {
      public string BaseUrl { get; set; } = string.Empty;
      public string? ApiKey { get; set; }
      public bool Enabled { get; set; } = true;
      public int TimeoutSeconds { get; set; } = 5;
  }
  ```

## Testing Requirements

### Test Naming Convention

```csharp
[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

### Test Structure (AAA Pattern)

```csharp
[Fact]
public async Task GetForecastAsync_WithValidLocation_ReturnsSuccessfulForecast()
{
    // Arrange
    var location = new Location("London", "GB");
    var provider = new OpenMeteoProvider(mockHttpClient, mockLogger);
    
    // Act
    var result = await provider.GetForecastAsync(location, DateTime.Today, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.Available.Should().BeTrue();
    result.Temperature.Should().NotBeNull();
}
```

### Mocking Guidelines

- Use **NSubstitute** for mocking interfaces
- Mock external dependencies (HTTP clients, database, cache)
- Don't mock domain entities or value objects
- Example:
  ```csharp
  var mockCache = Substitute.For<ICacheRepository>();
  mockCache.GetAsync<WeatherForecastResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
           .Returns((WeatherForecastResponse?)null);
  ```

### Test Data Generation

- Use **Bogus** for generating test data
- Create reusable test data builders
- Example:
  ```csharp
  var faker = new Faker<WeatherForecastRequest>()
      .RuleFor(r => r.City, f => f.Address.City())
      .RuleFor(r => r.Country, "GB")
      .RuleFor(r => r.Date, DateTime.Today);
  ```

## Observability and Logging

### Structured Logging

- Use **Serilog** with structured logging (JSON format)
- Log levels:
  - **Trace**: Detailed diagnostic information
  - **Debug**: Development debugging information
  - **Information**: General informational messages
  - **Warning**: Recoverable errors or unexpected behavior
  - **Error**: Unrecoverable errors
  - **Fatal**: Critical failures requiring immediate attention

### OpenTelemetry

- Instrumentation for ASP.NET Core, HttpClient, Redis
- Export to Azure Application Insights (production) or Console (development)
- Custom metrics: cache hit ratio, source availability, response time

### Health Checks

- **`/health`**: Detailed health check with all dependencies
- **`/health/ready`**: Readiness probe (Kubernetes)
- **`/health/live`**: Liveness probe (Kubernetes)
- Status levels: Healthy, Degraded (partial failure), Unhealthy (all failed)

## CI/CD Considerations

### GitHub Actions Workflow

The project uses GitHub Actions for CI/CD:

1. **Build and Test**: On all branches and PRs
2. **Docker Build and Push**: On main branch, push to Azure Container Registry
3. **Infrastructure Deployment**: Deploy Azure resources with Bicep
4. **Application Deployment**: Update Azure Container App
5. **Post-Deployment Tests**: Smoke tests against deployed endpoint

### Environment Variables

Required secrets for CI/CD:
- `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`: Azure authentication
- `WEATHERAPI_KEY`: WeatherAPI.com API key
- `OPENWEATHERMAP_KEY`: OpenWeatherMap API key

### Docker

- Multi-stage Dockerfile: build → publish → runtime
- Non-root user execution
- Health check endpoint
- Optimized image size with `.dockerignore`

## Common Tasks and Examples

### Adding a New Weather Source Provider

1. Create provider class in `Infrastructure/ExternalServices/`
2. Implement `IWeatherSourceProvider` interface
3. Add configuration to `appsettings.json`
4. Register in `Infrastructure/DependencyInjection.cs`
5. Add unit tests in `Infrastructure.Tests/ExternalServices/`

### Adding a New API Endpoint

1. Define request/response DTOs in `Application/Common/Models/`
2. Create validator in `Application/Validators/`
3. Add service interface and implementation in `Application/Services/`
4. Create controller action in `Api/Controllers/`
5. Add XML documentation comments for Swagger
6. Add unit tests for validator and service

### Modifying Business Logic

1. Update domain entities or value objects in `Domain/`
2. Update corresponding tests in `Domain.Tests/`
3. Update application services if needed
4. Run all tests to ensure no regressions

## Architecture Decision Records (ADRs)

The project follows Clean Architecture and Domain-Driven Design principles as outlined in the implementation plan (`plan/feature-weather-aggregation-microservice-1.md`). Significant architectural decisions should be documented as ADRs in a `docs/adr/` directory when needed.

## Security Best Practices

- **No secrets in source control**: Use `.gitignore` for `.env`, `appsettings.*.json` with secrets
- **API keys in environment variables**: Override configuration at runtime
- **Non-root container**: Docker container runs as non-root user
- **Minimal runtime image**: Use `aspnet` base image (no SDK)
- **HTTPS only**: Enforced by Azure Container Apps
- **Input validation**: All user input validated with FluentValidation
- **Dependency scanning**: Run `dotnet list package --vulnerable` regularly

## Common Pitfalls to Avoid

1. **Don't bypass dependency injection**: Always use DI, never use `new` for services
2. **Don't add business logic to DTOs**: Keep DTOs as simple data containers
3. **Don't reference higher layers from lower layers**: Domain should never reference Application/Infrastructure
4. **Don't catch and swallow exceptions**: Always log exceptions or rethrow
5. **Don't use `async void`**: Use `async Task` (except event handlers)
6. **Don't forget cancellation tokens**: Pass `CancellationToken` to async methods
7. **Don't mock value objects**: Test them directly, they're lightweight
8. **Don't add external dependencies to Domain**: Keep it pure

## Resources and References

### Official Documentation

- [Clean Architecture in ASP.NET Core](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/)
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)

### Design Patterns

- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/reference/)
- [Martin Fowler - Patterns of Enterprise Application Architecture](https://martinfowler.com/eaaCatalog/)

### Tools and Libraries

- [Polly](https://github.com/App-vNext/Polly) - Resilience and transient-fault-handling
- [FluentValidation](https://fluentvalidation.net/) - Validation library
- [Serilog](https://serilog.net/) - Structured logging
- [xUnit](https://xunit.net/) - Testing framework
- [FluentAssertions](https://fluentassertions.com/) - Assertion library
- [NSubstitute](https://nsubstitute.github.io/) - Mocking library
- [Bogus](https://github.com/bchavez/Bogus) - Test data generation

## Custom Agents and Tools

This repository has custom agents configured in `.github/agents/`:

- **address-comments**: Address PR comments and feedback
- **adr-generator**: Create comprehensive Architectural Decision Records
- **azure-principal-architect**: Azure architecture decisions
- **azure-saas-architect**: SaaS architecture patterns
- **CSharpExpert**: C# development guidance
- **csharp-dotnet-janitor**: Code cleanup and modernization
- **plan**: Strategic planning and implementation
- **principal-software-engineer**: Software engineering best practices
- **prd**: Generate Product Requirements Documents
- **task-researcher**: Comprehensive project analysis
- **wg-code-alchemist**: Code transformation with Clean Code principles

Use these agents via GitHub Copilot when working on specific tasks related to their expertise.

## Getting Help

For questions or issues:

1. Check this instructions file first
2. Review the implementation plan in `plan/feature-weather-aggregation-microservice-1.md`
3. Use the custom agents for specialized guidance
4. Refer to the official documentation links above
