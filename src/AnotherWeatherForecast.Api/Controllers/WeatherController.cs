using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AnotherWeatherForecast.Application.Common.Interfaces;
using AnotherWeatherForecast.Application.Common.Models;
using ProblemDetails = AnotherWeatherForecast.Application.Common.Models.ProblemDetails;

namespace AnotherWeatherForecast.Api.Controllers;

/// <summary>
/// Controller for aggregating weather forecasts from multiple sources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherAggregationService _aggregationService;
    private readonly IValidator<WeatherForecastRequest> _validator;
    private readonly ILogger<WeatherController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherController"/> class.
    /// </summary>
    /// <param name="aggregationService">The weather aggregation service.</param>
    /// <param name="validator">The request validator.</param>
    /// <param name="logger">The logger.</param>
    public WeatherController(
        IWeatherAggregationService aggregationService,
        IValidator<WeatherForecastRequest> validator,
        ILogger<WeatherController> logger)
    {
        _aggregationService = aggregationService ?? throw new ArgumentNullException(nameof(aggregationService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets aggregated weather forecast from multiple sources.
    /// </summary>
    /// <param name="date">The forecast date (ISO 8601 format). Due to limitations of some sources, only +7 days from the current date are supported and historical data is limited to the past 2 months.</param>
    /// <param name="city">The city name.</param>
    /// <param name="country">The country code (ISO 3166-1).</param>
    /// <param name="sources">Optional array of source names to query. If not provided, all sources are queried.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregated weather forecast response with temperature, humidity, and metadata.</returns>
    /// <response code="200">Returns the aggregated weather forecast.</response>
    /// <response code="400">If the request validation fails (e.g., invalid date range, missing fields).</response>
    /// <response code="503">If all weather sources are unavailable.</response>
    [HttpGet("forecast")]
    [ProducesResponseType(typeof(WeatherForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetForecast(
        [FromQuery] DateTime date,
        [FromQuery] string city,
        [FromQuery] string country,
        [FromQuery] string sources = null,
        CancellationToken cancellationToken = default)
    {
        var request = new WeatherForecastRequest
        {
            Date = date,
            City = city,
            Country = country,
            Sources = string.IsNullOrWhiteSpace(sources)
                ? null
                : sources.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
        };

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        var response = await _aggregationService.GetAggregatedForecastAsync(request, cancellationToken);

        if(response.Sources.Count == 0)
        {
            _logger.LogError("No weather sources found for {City}, {Country}. Sources: {Sources}", city, country, sources != null ? string.Join(", ", sources) : "All");

            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Title = "Invalid sources",
                    Detail = "No matching weather source providers found for the requested sources.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
        }
        if (response.Sources.All(s => !s.Available))
        {
            _logger.LogError("All weather sources unavailable for {City}, {Country}", city, country);

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Service Unavailable",
                    Detail = "All weather sources are currently unavailable. Please try again later.",
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Instance = HttpContext.Request.Path
                });
        }

        return Ok(response);
    }
}
