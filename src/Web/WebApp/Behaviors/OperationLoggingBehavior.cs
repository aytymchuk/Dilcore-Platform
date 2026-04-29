using System.Diagnostics;
using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Extensions;
using FluentResults;
using MediatR;

namespace Dilcore.WebApp.Behaviors;

internal class OperationLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly string RequestName = typeof(TRequest).Name;
    private static readonly bool IsCommand;
    private static readonly bool IsQuery;

    static OperationLoggingBehavior()
    {
        var type = typeof(TRequest);
        var interfaces = type.GetInterfaces();

        IsCommand = interfaces.Any(i =>
            i == typeof(ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));

        IsQuery = !IsCommand && interfaces.Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
    }

    private readonly ILogger<OperationLoggingBehavior<TRequest, TResponse>> _logger;

    public OperationLoggingBehavior(ILogger<OperationLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        if (IsQuery)
        {
            _logger.LogLoadingData(RequestName);
        }
        else if (IsCommand)
        {
            _logger.LogSavingData(RequestName);
        }

        try
        {
            var response = await next(cancellationToken);
            sw.Stop();

            if (response is IResultBase resultBase && resultBase.IsFailed)
            {
                var error = string.Join(", ", resultBase.Errors.Select(e => e.Message));

                if (IsQuery)
                {
                    _logger.LogLoadingDataFailed(RequestName, sw.ElapsedMilliseconds, error);
                }
                else if (IsCommand)
                {
                    _logger.LogSavingDataFailed(RequestName, sw.ElapsedMilliseconds, error);
                }

                return response;
            }

            if (IsQuery)
            {
                _logger.LogLoadingDataSucceeded(RequestName, sw.ElapsedMilliseconds);
            }
            else if (IsCommand)
            {
                _logger.LogSavingDataSucceeded(RequestName, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            if (IsQuery)
            {
                _logger.LogLoadingDataException(ex, RequestName, sw.ElapsedMilliseconds);
            }
            else if (IsCommand)
            {
                _logger.LogSavingDataException(ex, RequestName, sw.ElapsedMilliseconds);
            }

            throw;
        }
    }
}

