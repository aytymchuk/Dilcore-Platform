using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.WebApi.Features.WeatherForecast;

public class CreateWeatherForecastCommand : ICommand<WeatherForecast>
{
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}
