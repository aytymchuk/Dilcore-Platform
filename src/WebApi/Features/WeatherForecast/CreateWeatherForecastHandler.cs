using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.WebApi.Features.WeatherForecast;

public class CreateWeatherForecastHandler : ICommandHandler<CreateWeatherForecastCommand, WeatherForecast>
{
    private readonly TimeProvider _timeProvider;

    public CreateWeatherForecastHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task<Result<WeatherForecast>> Handle(CreateWeatherForecastCommand request, CancellationToken cancellationToken)
    {
        var forecast = new WeatherForecast(
            DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime),
            request.TemperatureC,
            request.Summary);

        return Task.FromResult(Result.Ok(forecast));
    }
}
