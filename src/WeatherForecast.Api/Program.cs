
using WeatherForecast.Application;
using WeatherForecast.Infrastructure;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Application.Common.Models;
using WeatherForecast.Domain.ValueObjects;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Debug endpoint: aggregate weather for London, GB, today
app.MapGet("/aggregate", async (IWeatherAggregationService aggregator) =>
{
    var request = new WeatherForecastRequest
    {
        City = "London",
        Country = "GB",
        Date = DateTime.Today
    };
    var result = await aggregator.GetAggregatedForecastAsync(request, CancellationToken.None);
    return Results.Json(result);
})
.WithName("AggregateWeatherDebug")
.WithOpenApi();

app.Run();
