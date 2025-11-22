using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Domain.Tests;

public class WeatherForecastTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesWeatherForecast()
    {
        // Arrange
        var location = new Location("Paris", "FR");
        var date = DateTime.UtcNow.Date;

        // Act
        var forecast = new AnotherWeatherForecast.Domain.Entities.WeatherForecast(location, date);

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
        var forecast = new AnotherWeatherForecast.Domain.Entities.WeatherForecast(location, dateTime);

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
        Assert.Throws<ArgumentNullException>(() => new AnotherWeatherForecast.Domain.Entities.WeatherForecast(null!, date));
    }

    [Fact]
    public void AddSource_WithValidSource_AddsSourceToCollection()
    {
        // Arrange
        var location = new Location("Tokyo", "JP");
        var date = DateTime.UtcNow.Date;
        var forecast = new AnotherWeatherForecast.Domain.Entities.WeatherForecast(location, date);
        var source = new ForecastSource(
            "OpenMeteo",
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
        var forecast = new AnotherWeatherForecast.Domain.Entities.WeatherForecast(location, date);
        var source1 = new ForecastSource(
            "OpenMeteo",
            new Temperature(20m),
            new Humidity(65m),
            available: true,
            error: null,
            DateTime.UtcNow);
        var source2 = new ForecastSource(
            "WeatherAPI",
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
        var forecast = new AnotherWeatherForecast.Domain.Entities.WeatherForecast(location, date);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => forecast.AddSource(null!));
    }

    [Fact]
    public void Sources_ReturnsReadOnlyCollection()
    {
        // Arrange
        var location = new Location("Rome", "IT");
        var date = DateTime.UtcNow.Date;
        var forecast = new AnotherWeatherForecast.Domain.Entities.WeatherForecast(location, date);

        // Act
        var sources = forecast.Sources;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<ForecastSource>>(sources);
    }
}
