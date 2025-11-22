using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Domain.Tests;

public class ForecastSourceTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesForecastSource()
    {
        // Arrange
        var sourceName = "OpenMeteo";
        var temperature = new Temperature(20m);
        var humidity = new Humidity(65m);
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            sourceName,
            temperature,
            humidity,
            available: true,
            error: null,
            retrievedAt);

        // Assert
        Assert.Equal(sourceName, source.SourceName);
        Assert.Equal(temperature, source.Temperature);
        Assert.Equal(humidity, source.Humidity);
        Assert.True(source.Available);
        Assert.Null(source.Error);
        Assert.Equal(retrievedAt, source.RetrievedAt);
    }

    [Fact]
    public void Constructor_WithNullTemperatureAndHumidity_CreatesForecastSource()
    {
        // Arrange
        var sourceName = "WeatherAPI";
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            sourceName,
            temperature: null,
            humidity: null,
            available: false,
            error: "Service unavailable",
            retrievedAt);

        // Assert
        Assert.Equal(sourceName, source.SourceName);
        Assert.Null(source.Temperature);
        Assert.Null(source.Humidity);
        Assert.False(source.Available);
        Assert.Equal("Service unavailable", source.Error);
        Assert.Equal(retrievedAt, source.RetrievedAt);
    }

    [Fact]
    public void Constructor_WithErrorMessage_StoresError()
    {
        // Arrange
        var sourceName = "OpenMeteo";
        var errorMessage = "Connection timeout";
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            sourceName,
            temperature: null,
            humidity: null,
            available: false,
            error: errorMessage,
            retrievedAt);

        // Assert
        Assert.Equal(errorMessage, source.Error);
        Assert.False(source.Available);
    }
}
