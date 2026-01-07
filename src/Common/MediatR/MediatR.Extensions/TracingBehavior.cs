using System.Diagnostics;
using MediatR;

namespace Dilcore.MediatR.Extensions;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource ActivitySource = new("Application.Operations");

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        using var activity = ActivitySource.StartActivity($"MediatR: {requestName}", ActivityKind.Internal);

        if (activity != null)
        {
            activity.SetTag("mediatr.request_name", requestName);
            activity.SetTag("mediatr.request_type", typeof(TRequest).FullName);
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