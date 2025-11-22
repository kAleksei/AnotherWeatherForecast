namespace WeatherForecast.Infrastructure.Responses;

public sealed record OpenWeatherMapHistoryResponse(
    int CityId,
    int Count,
    List<OpenWeatherMapHistoryItem> List);
public sealed record OpenWeatherMapHistoryItem(
    OpenWeatherMapMain Main);

public sealed record OpenWeatherMapMain(
    decimal Temp,
    decimal Humidity);
