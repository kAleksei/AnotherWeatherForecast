using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Enums;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Domain.Tests;

public class ForecastSourceTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesForecastSource()
    {
        // Arrange
        var name = "OpenMeteo";
        var sourceType = WeatherSourceType.OpenMeteo;
        var temperature = new Temperature(20m);
        var humidity = new Humidity(65m);
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            name,
            sourceType,
            temperature,
            humidity,
            available: true,
            error: null,
            retrievedAt);

        // Assert
        Assert.Equal(name, source.Name);
        Assert.Equal(sourceType, source.SourceType);
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
        var name = "WeatherAPI";
        var sourceType = WeatherSourceType.WeatherAPI;
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            name,
            sourceType,
            temperature: null,
            humidity: null,
            available: false,
            error: "Service unavailable",
            retrievedAt);

        // Assert
        Assert.Equal(name, source.Name);
        Assert.Equal(sourceType, source.SourceType);
        Assert.Null(source.Temperature);
        Assert.Null(source.Humidity);
        Assert.False(source.Available);
        Assert.Equal("Service unavailable", source.Error);
        Assert.Equal(retrievedAt, source.RetrievedAt);
    }

    [Fact]
    public void Constructor_WithWhitespaceName_TrimsName()
    {
        // Arrange
        var name = "  OpenWeatherMap  ";
        var sourceType = WeatherSourceType.OpenWeatherMap;
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            name,
            sourceType,
            temperature: null,
            humidity: null,
            available: true,
            error: null,
            retrievedAt);

        // Assert
        Assert.Equal("OpenWeatherMap", source.Name);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var sourceType = WeatherSourceType.OpenMeteo;
        var retrievedAt = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ForecastSource(
            null!,
            sourceType,
            temperature: null,
            humidity: null,
            available: true,
            error: null,
            retrievedAt));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var sourceType = WeatherSourceType.OpenMeteo;
        var retrievedAt = DateTime.UtcNow;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ForecastSource(
            "",
            sourceType,
            temperature: null,
            humidity: null,
            available: true,
            error: null,
            retrievedAt));
        Assert.Contains("Name cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var sourceType = WeatherSourceType.OpenMeteo;
        var retrievedAt = DateTime.UtcNow;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ForecastSource(
            "   ",
            sourceType,
            temperature: null,
            humidity: null,
            available: true,
            error: null,
            retrievedAt));
        Assert.Contains("Name cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData(WeatherSourceType.OpenMeteo)]
    [InlineData(WeatherSourceType.WeatherAPI)]
    [InlineData(WeatherSourceType.OpenWeatherMap)]
    public void Constructor_WithDifferentSourceTypes_CreatesForecastSource(WeatherSourceType sourceType)
    {
        // Arrange
        var name = "Test Source";
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            name,
            sourceType,
            temperature: null,
            humidity: null,
            available: true,
            error: null,
            retrievedAt);

        // Assert
        Assert.Equal(sourceType, source.SourceType);
    }

    [Fact]
    public void Constructor_WithErrorMessage_StoresError()
    {
        // Arrange
        var name = "Test Source";
        var sourceType = WeatherSourceType.OpenMeteo;
        var errorMessage = "Connection timeout";
        var retrievedAt = DateTime.UtcNow;

        // Act
        var source = new ForecastSource(
            name,
            sourceType,
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
