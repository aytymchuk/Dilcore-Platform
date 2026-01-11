using FluentResults;
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
            var response = await next(cancellationToken);

            if (response is IResultBase result)
            {
                if (result.IsFailed)
                {
                    var errorMessages = new List<string>();
                    ProcessErrors(result.Errors, errorMessages, requestName);

                    _logger.LogRequestFailed(requestName, string.Join(", ", errorMessages));
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

    private void ProcessErrors(IEnumerable<IReason> reasons, List<string> messages, string requestName)
    {
        foreach (var reason in reasons)
        {
            if (reason is IError error)
            {
                messages.Add(error.Message);

                if (error is ExceptionalError exceptionalError)
                {
                    _logger.LogRequestFailedWithException(exceptionalError.Exception, requestName, error.Message);
                }

                ProcessErrors(error.Reasons, messages, requestName);
            }
        }
    }
}