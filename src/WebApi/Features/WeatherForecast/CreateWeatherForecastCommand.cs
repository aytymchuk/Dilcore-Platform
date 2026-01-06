using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.WebApi.Features.WeatherForecast;

public record CreateWeatherForecastCommand : ICommand<WeatherForecast>
{
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
}