using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AnotherWeatherForecast.Infrastructure.Responses;

/// <summary>
/// Response model for OpenWeatherMap 5-day/3-hour forecast API.
/// </summary>
public sealed record OpenWeatherMapForecastResponse(
    [property: JsonPropertyName("cnt")] int Count,
    [property: JsonPropertyName("list")] List<OpenWeatherMapForecastItem> List);

public sealed record OpenWeatherMapForecastItem(
    [property: JsonPropertyName("dt")] long Dt,
    [property: JsonPropertyName("main")] OpenWeatherMapForecastMain Main);

public sealed record OpenWeatherMapForecastMain(
    [property: JsonPropertyName("temp")] decimal Temp,
    [property: JsonPropertyName("humidity")] decimal Humidity);