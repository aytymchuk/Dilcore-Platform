using Microsoft.Extensions.Logging;

namespace WebApi.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Getting weather forecast for {Count} days")]
    public static partial void LogGettingWeatherForecast(this ILogger logger, int count);
}