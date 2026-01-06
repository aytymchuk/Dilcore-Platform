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
        var referenceDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeEmpty();
        result.Value.Count().ShouldBe(5);

        // Check dates are sequential starting from reference date
        result.Value.First().Date.ShouldBeGreaterThanOrEqualTo(referenceDate);

        // Verify Logging
        logger.Logs.ShouldContain(l => l.Contains("Getting weather forecast for 5 days"));
    }

    [Test]
    public async Task CreateWeatherForecast_ShouldReturnRequestData()
    {
        // Arrange
        var fixedDate = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(fixedDate);
        var handler = new CreateWeatherForecastHandler(fakeTimeProvider);
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
        result.Value.Date.ShouldBe(DateOnly.FromDateTime(fixedDate.UtcDateTime));
        result.Value.TemperatureF.ShouldBe(76); // 32 + (int)(25 / 0.5556) = 32 + 44 = 76
    }

    /// <summary>
    /// Simple fake TimeProvider for testing with a fixed time.
    /// </summary>
    private class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _fixedTime;

        public FakeTimeProvider(DateTimeOffset fixedTime)
        {
            _fixedTime = fixedTime;
        }

        public override DateTimeOffset GetUtcNow() => _fixedTime;
    }
}
