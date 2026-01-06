using Dilcore.MediatR.Abstractions;
using FluentResults;
using Dilcore.WebApi.Extensions;
using Microsoft.Extensions.Logging;

namespace Dilcore.WebApi.Features.WeatherForecast;

public class GetWeatherForecastHandler : IQueryHandler<GetWeatherForecastQuery, IEnumerable<WeatherForecast>>
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<GetWeatherForecastHandler> _logger;

    public GetWeatherForecastHandler(ILogger<GetWeatherForecastHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result<IEnumerable<WeatherForecast>>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        _logger.LogGettingWeatherForecast(5);

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToArray();

        return Task.FromResult(Result.Ok<IEnumerable<WeatherForecast>>(forecast));
    }
}
