using Dilcore.WebApi.Features.WeatherForecast;
using Shouldly;
using Dilcore.Tests.Common;

namespace Dilcore.WebApi.Tests.Features.WeatherForecast;

public class WeatherForecastTests
{
    [Test]
    public async Task GetWeatherForecast_ShouldReturnForecastsAndLog()
    {
        // Arrange
        var logger = new ListLogger<GetWeatherForecastHandler>();
        var handler = new GetWeatherForecastHandler(logger);
        var query = new GetWeatherForecastQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeEmpty();
        result.Value.Count().ShouldBe(5);

        // Check dates are mostly sequential/future (simple check)
        result.Value.First().Date.ShouldBeGreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now));

        // Verify Logging
        logger.Logs.ShouldContain(l => l.Contains("Getting weather forecast for 5 days"));
    }

    [Test]
    public async Task CreateWeatherForecast_ShouldReturnRequestData()
    {
        // Arrange
        var handler = new CreateWeatherForecastHandler(TimeProvider.System);
        var command = new CreateWeatherForecastCommand
        {
            TemperatureC = 25,
            Summary = "Perfect"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TemperatureC.ShouldBe(25);
        result.Value.Summary.ShouldBe("Perfect");
        result.Value.Date.ShouldBe(DateOnly.FromDateTime(DateTime.Now));
        result.Value.TemperatureF.ShouldBe(76); // 32 + (int)(25 / 0.5556) = 32 + 44 = 76
    }
}


