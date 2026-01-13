using Dilcore.Extensions.OpenApi.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Extensions.OpenApi;

public sealed class DilcoreOpenApiBuilder(IServiceCollection services) : IDilcoreOpenApiBuilder
{
    public IServiceCollection Services => services;
}
