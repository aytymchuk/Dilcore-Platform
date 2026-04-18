using Dilcore.MediatR.Abstractions;
using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.Tenancy.Contracts.Tenants.Create;
using Dilcore.WebApi.Client.Clients;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.Create;

/// <summary>
/// Handler for creating a new tenant.
/// </summary>
public class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, TenantDto>
{
    private readonly ITenancyClient _tenancyClient;

    public CreateTenantCommandHandler(ITenancyClient tenancyClient)
    {
        _tenancyClient = tenancyClient;
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var parameters = request.Parameters;
        
        var dto = new CreateTenantDto
        {
            Name = parameters.Name,
            Description = parameters.Description
        };

        return await _tenancyClient.CreateTenantAsync(dto);
    }
}
