using System.Text.Json;
using System.Text.Json.Serialization;
using Dilcore.Configuration.Extensions;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Refit;

namespace Dilcore.WebApp.Http.AiAgent;

/// <summary>
/// Registers the blueprint agent Refit client and facade.
/// </summary>
public static class AiAgentServiceCollectionExtensions
{
    /// <summary>
    /// Adds AI Agent API client services (Refit + <see cref="IBlueprintsAgentService"/>).
    /// Requires <see cref="AccessTokenDelegatingHandler"/> to be registered (e.g. via platform API setup).
    /// </summary>
    public static IServiceCollection AddAiAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSettings<AgentApiSettings>();
        services.AddSingleton(settings);

        var jsonOptions = CreateAgentJsonSerializerOptions();
        services.AddSingleton(jsonOptions);

        services.AddRefitClient<IBlueprintsAgentClient>(new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = settings.BaseUrl;
                c.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<AccessTokenDelegatingHandler>();

        services.AddScoped<IBlueprintsAgentService, BlueprintsAgentService>();

        return services;
    }

    private static JsonSerializerOptions CreateAgentJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new ThreadActionResponseDtoConverter());

        return options;
    }
}
