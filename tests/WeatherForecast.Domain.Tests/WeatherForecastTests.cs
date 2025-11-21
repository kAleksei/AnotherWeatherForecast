using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Enums;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Domain.Tests;

public class WeatherForecastTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesWeatherForecast()
    {
        // Arrange
        var location = new Location("Paris", "FR");
        var date = DateTime.UtcNow.Date;

        // Act
        var forecast = new Domain.Entities.WeatherForecast(location, date);

        // Assert
        Assert.Equal(location, forecast.Location);
        Assert.Equal(date, forecast.Date);
        Assert.Empty(forecast.Sources);
    }

    [Fact]
    public void Constructor_NormalizesToDate_IgnoresTime()
    {
        // Arrange
        var location = new Location("London", "GB");
        var dateTime = DateTime.UtcNow.AddHours(12).AddMinutes(30);

        // Act
        var forecast = new Domain.Entities.WeatherForecast(location, dateTime);

        // Assert
        Assert.Equal(dateTime.Date, forecast.Date);
        Assert.Equal(TimeSpan.Zero, forecast.Date.TimeOfDay);
    }

    [Fact]
    public void Constructor_WithNullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Domain.Entities.WeatherForecast(null!, date));
    }

    [Fact]
    public void AddSource_WithValidSource_AddsSourceToCollection()
    {
        // Arrange
        var location = new Location("Tokyo", "JP");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var source = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            new Temperature(25m),
            new Humidity(60m),
            available: true,
            error: null,
            DateTime.UtcNow);

        // Act
        forecast.AddSource(source);

        // Assert
        Assert.Single(forecast.Sources);
        Assert.Contains(source, forecast.Sources);
    }

    [Fact]
    public void AddSource_WithMultipleSources_AddsAllSources()
    {
        // Arrange
        var location = new Location("Berlin", "DE");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var source1 = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            new Temperature(20m),
            new Humidity(65m),
            available: true,
            error: null,
            DateTime.UtcNow);
        var source2 = new ForecastSource(
            "WeatherAPI",
            WeatherSourceType.WeatherAPI,
            new Temperature(22m),
            new Humidity(70m),
            available: true,
            error: null,
            DateTime.UtcNow);

        // Act
        forecast.AddSource(source1);
        forecast.AddSource(source2);

        // Assert
        Assert.Equal(2, forecast.Sources.Count);
        Assert.Contains(source1, forecast.Sources);
        Assert.Contains(source2, forecast.Sources);
    }

    [Fact]
    public void AddSource_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var location = new Location("Madrid", "ES");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => forecast.AddSource(null!));
    }

    [Fact]
    public void Sources_ReturnsReadOnlyCollection()
    {
        // Arrange
        var location = new Location("Rome", "IT");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);

        // Act
        var sources = forecast.Sources;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<ForecastSource>>(sources);
    }

    [Fact]
    public void CalculateAverage_WithNoSources_ReturnsNull()
    {
        // Arrange
        var location = new Location("Amsterdam", "NL");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);

        // Act
        var (avgTemp, avgHumidity) = forecast.CalculateAverage();

        // Assert
        Assert.Null(avgTemp);
        Assert.Null(avgHumidity);
    }

    [Fact]
    public void CalculateAverage_WithOnlyUnavailableSources_ReturnsNull()
    {
        // Arrange
        var location = new Location("Brussels", "BE");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var source = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            temperature: null,
            humidity: null,
            available: false,
            error: "Service down",
            DateTime.UtcNow);
        forecast.AddSource(source);

        // Act
        var (avgTemp, avgHumidity) = forecast.CalculateAverage();

        // Assert
        Assert.Null(avgTemp);
        Assert.Null(avgHumidity);
    }

    [Fact]
    public void CalculateAverage_WithOneAvailableSource_ReturnsSourceValues()
    {
        // Arrange
        var location = new Location("Vienna", "AT");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var temperature = new Temperature(23m);
        var humidity = new Humidity(55m);
        var source = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            temperature,
            humidity,
            available: true,
            error: null,
            DateTime.UtcNow);
        forecast.AddSource(source);

        // Act
        var (avgTemp, avgHumidity) = forecast.CalculateAverage();

        // Assert
        Assert.NotNull(avgTemp);
        Assert.NotNull(avgHumidity);
        Assert.Equal(23m, avgTemp.Celsius);
        Assert.Equal(55m, avgHumidity.Percent);
    }

    [Fact]
    public void CalculateAverage_WithMultipleAvailableSources_ReturnsAverageValues()
    {
        // Arrange
        var location = new Location("Copenhagen", "DK");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var source1 = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            new Temperature(20m),
            new Humidity(60m),
            available: true,
            error: null,
            DateTime.UtcNow);
        var source2 = new ForecastSource(
            "WeatherAPI",
            WeatherSourceType.WeatherAPI,
            new Temperature(24m),
            new Humidity(70m),
            available: true,
            error: null,
            DateTime.UtcNow);
        forecast.AddSource(source1);
        forecast.AddSource(source2);

        // Act
        var (avgTemp, avgHumidity) = forecast.CalculateAverage();

        // Assert
        Assert.NotNull(avgTemp);
        Assert.NotNull(avgHumidity);
        Assert.Equal(22m, avgTemp.Celsius); // (20 + 24) / 2
        Assert.Equal(65m, avgHumidity.Percent); // (60 + 70) / 2
    }

    [Fact]
    public void CalculateAverage_WithMixedAvailability_CalculatesOnlyFromAvailableSources()
    {
        // Arrange
        var location = new Location("Stockholm", "SE");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var source1 = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            new Temperature(18m),
            new Humidity(50m),
            available: true,
            error: null,
            DateTime.UtcNow);
        var source2 = new ForecastSource(
            "WeatherAPI",
            WeatherSourceType.WeatherAPI,
            temperature: null,
            humidity: null,
            available: false,
            error: "Timeout",
            DateTime.UtcNow);
        var source3 = new ForecastSource(
            "OpenWeatherMap",
            WeatherSourceType.OpenWeatherMap,
            new Temperature(22m),
            new Humidity(60m),
            available: true,
            error: null,
            DateTime.UtcNow);
        forecast.AddSource(source1);
        forecast.AddSource(source2);
        forecast.AddSource(source3);

        // Act
        var (avgTemp, avgHumidity) = forecast.CalculateAverage();

        // Assert
        Assert.NotNull(avgTemp);
        Assert.NotNull(avgHumidity);
        Assert.Equal(20m, avgTemp.Celsius); // (18 + 22) / 2
        Assert.Equal(55m, avgHumidity.Percent); // (50 + 60) / 2
    }

    [Fact]
    public void CalculateAverage_WithPartialData_ReturnsAverageOfAvailableData()
    {
        // Arrange
        var location = new Location("Oslo", "NO");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var source1 = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            new Temperature(15m),
            humidity: null,
            available: true,
            error: null,
            DateTime.UtcNow);
        var source2 = new ForecastSource(
            "WeatherAPI",
            WeatherSourceType.WeatherAPI,
            temperature: null,
            new Humidity(75m),
            available: true,
            error: null,
            DateTime.UtcNow);
        forecast.AddSource(source1);
        forecast.AddSource(source2);

        // Act
        var (avgTemp, avgHumidity) = forecast.CalculateAverage();

        // Assert
        Assert.NotNull(avgTemp);
        Assert.NotNull(avgHumidity);
        Assert.Equal(15m, avgTemp.Celsius); // Only one temperature value
        Assert.Equal(75m, avgHumidity.Percent); // Only one humidity value
    }

    [Fact]
    public void CalculateAverage_WithThreeSources_CalculatesCorrectAverage()
    {
        // Arrange
        var location = new Location("Helsinki", "FI");
        var date = DateTime.UtcNow.Date;
        var forecast = new Domain.Entities.WeatherForecast(location, date);
        var source1 = new ForecastSource(
            "OpenMeteo",
            WeatherSourceType.OpenMeteo,
            new Temperature(10m),
            new Humidity(40m),
            available: true,
            error: null,
            DateTime.UtcNow);
        var source2 = new ForecastSource(
            "WeatherAPI",
            WeatherSourceType.WeatherAPI,
            new Temperature(20m),
            new Humidity(50m),
            available: true,
            error: null,
            DateTime.UtcNow);
        var source3 = new ForecastSource(
            "OpenWeatherMap",
            WeatherSourceType.OpenWeatherMap,
            new Temperature(30m),
            new Humidity(60m),
            available: true,
            error: null,
            DateTime.UtcNow);
        forecast.AddSource(source1);
        forecast.AddSource(source2);
        forecast.AddSource(source3);

        // Act
        var (avgTemp, avgHumidity) = forecast.CalculateAverage();

        // Assert
        Assert.NotNull(avgTemp);
        Assert.NotNull(avgHumidity);
        Assert.Equal(20m, avgTemp.Celsius); // (10 + 20 + 30) / 3
        Assert.Equal(50m, avgHumidity.Percent); // (40 + 50 + 60) / 3
    }
}
