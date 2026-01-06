using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.WebApi.Features.WeatherForecast;

public class GetWeatherForecastQuery : IQuery<IEnumerable<WeatherForecast>>
{
}
