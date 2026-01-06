using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.WebApi.Features.WeatherForecast;

public class CreateWeatherForecastHandler : ICommandHandler<CreateWeatherForecastCommand, WeatherForecast>
{
    public Task<Result<WeatherForecast>> Handle(CreateWeatherForecastCommand request, CancellationToken cancellationToken)
    {
        var forecast = new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now),
            request.TemperatureC,
            request.Summary);

        return Task.FromResult(Result.Ok(forecast));
    }
}
