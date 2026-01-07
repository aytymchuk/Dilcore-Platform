using System.Diagnostics;
using System.Text.Json;
using Dilcore.MediatR.Abstractions;
using MediatR;

namespace Dilcore.MediatR.Extensions;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource ActivitySource = new("Application.Operations");

    // Cache reflection results per TRequest type
    private static readonly string RequestName = typeof(TRequest).Name;
    private static readonly string RequestFullName = typeof(TRequest).FullName ?? typeof(TRequest).Name;
    private static readonly bool IsCommand;
    private static readonly bool IsQuery;
    private static readonly string ActivityName;
    private static readonly ActivityKind ActivityKind;

    static TracingBehavior()
    {
        var type = typeof(TRequest);
        var interfaces = type.GetInterfaces();

        IsCommand = interfaces.Any(i =>
            i == typeof(ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));

        IsQuery = !IsCommand && interfaces.Any(i =>
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)));

        ActivityName = IsCommand ? $"Command: {RequestName}" :
                       IsQuery ? $"Query: {RequestName}" :
                       $"MediatR: {RequestName}";

        ActivityKind = IsQuery ? ActivityKind.Client : ActivityKind.Internal;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityName, ActivityKind);

        if (activity != null)
        {
            activity.SetTag("mediatr.request_name", RequestName);
            activity.SetTag("mediatr.request_type", RequestFullName);

            if (IsQuery)
            {
                activity.SetTag("db.system", "mediatr");
                activity.SetTag("db.statement", JsonSerializer.Serialize(request));
            }
        }

        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.AddEvent(new ActivityEvent("exception"));
            }
            throw;
        }
    }
}