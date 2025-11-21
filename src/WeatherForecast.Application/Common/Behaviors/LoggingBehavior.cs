using System.Diagnostics;
using WeatherForecast.Application.Common.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace WeatherForecast.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that provides logging and distributed tracing for requests.
/// </summary>
/// <typeparam name="TRequest">The type of request being logged.</typeparam>
/// <typeparam name="TResponse">The type of response expected from the request handler.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = TelemetryConstants.ActivitySource.StartActivity($"MediatR.{requestName}", ActivityKind.Internal);
        activity?.SetTag("request.type", requestName);

        try
        {
            var response = await next();
            
            activity?.SetStatus(ActivityStatusCode.Ok);

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            
            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}
