using MediatR;
using Microsoft.Extensions.Logging;

namespace Dilcore.MediatR.Extensions;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogHandlingRequest(requestName);

        try
        {
            var response = await next();

            if (response is FluentResults.IResultBase result)
            {
                if (result.IsFailed)
                {
                    _logger.LogRequestFailed(requestName, string.Join(", ", result.Reasons.Select(r => r.Message)));
                }
                else
                {
                    _logger.LogHandledRequest(requestName);
                }
            }
            else
            {
                _logger.LogHandledRequest(requestName);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogErrorHandlingRequest(ex, requestName);
            throw;
        }
    }
}