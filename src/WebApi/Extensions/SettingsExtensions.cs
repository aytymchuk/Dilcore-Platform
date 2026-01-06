using Dilcore.Configuration.Extensions;
using Dilcore.WebApi.Settings;

namespace Dilcore.WebApi.Extensions;

public static class SettingsExtensions
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterConfiguration<ApplicationSettings>(configuration);
        services.RegisterConfiguration<AuthenticationSettings>(configuration);

        return services;
    }
}