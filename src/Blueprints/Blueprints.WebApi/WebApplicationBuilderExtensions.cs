using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Core;
using Dilcore.Blueprints.Store;
using FluentValidation;
using Microsoft.AspNetCore.Builder;

namespace Dilcore.Blueprints.WebApi;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddBlueprintsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddBlueprintsApplication();
        builder.Services.AddBlueprintsStore(builder.Configuration);
        builder.Services.AddValidatorsFromAssembly(typeof(FieldDefinitionDtoValidator).Assembly);

        return builder;
    }
}
