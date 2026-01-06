using Dilcore.MediatR.Abstractions;

namespace Dilcore.WebApi.Features.WeatherForecast;

public record GetWeatherForecastQuery : IQuery<IEnumerable<WeatherForecast>>;