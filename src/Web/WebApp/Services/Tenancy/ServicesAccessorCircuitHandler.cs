using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Dilcore.WebApp.Services.Tenancy;

internal sealed class ServicesAccessorCircuitHandler : CircuitHandler
{
    private readonly IServiceProvider _services;
    private readonly CircuitServicesAccessor _accessor;

    public ServicesAccessorCircuitHandler(IServiceProvider services, CircuitServicesAccessor accessor)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            _accessor.Services = _services;
            try
            {
                await next(context);
            }
            finally
            {
                _accessor.Services = null;
            }
        };
    }
}
