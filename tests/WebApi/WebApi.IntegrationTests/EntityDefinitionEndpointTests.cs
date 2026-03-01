using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Update;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using Dilcore.WebApi.Client.Extensions;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Refit;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

[TestFixture]
public class EntityDefinitionEndpointTests
{
    private CustomWebApplicationFactory _factory = null!;
    private IBlueprintsClient _client = null!;
    private IDisposable _disposableClient = null!;
    private string _tenantId = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _tenantId = $"bp-tenant-{Guid.CreateVersion7():N}";

        _factory.FakeUser.UserId = $"bp-user-{Guid.CreateVersion7():N}";
        _factory.FakeUser.TenantId = _tenantId;

        await BaseIntegrationTest.SeedUserAndTenantAsync(_factory, _factory.FakeUser);
    }

    [SetUp]
    public void SetUp()
    {
        _factory.FakeUser.IsAuthenticated = true;
        var typedClient = _factory.CreateTypedClient<IBlueprintsClient>(_tenantId);
        _disposableClient = typedClient;
        _client = typedClient.Client;
    }

    [TearDown]
    public void TearDown()
    {
        _disposableClient?.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }

    private static CreateEntityDefinitionDto NewCreateDto(
        string? displayName = null,
        string? description = null,
        List<FieldDefinitionDto>? fields = null) => new()
    {
        DisplayName = displayName ?? $"Entity {Guid.CreateVersion7():N}",
        Description = description ?? "Test entity",
        Fields = fields ?? []
    };

    private static int GetStatusCode(FluentResults.ResultBase result) =>
        result.Errors.OfType<ApiError>().First().StatusCode;

    #region POST /blueprints/entity-definitions

    [Test]
    public async Task Create_ShouldReturn201_WhenValid()
    {
        var request = NewCreateDto(displayName: "Customer Profile");

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.DisplayName.ShouldBe("Customer Profile");
        result.Value.SchemaName.ShouldBe("customerProfile");
        result.Value.Description.ShouldBe("Test entity");
        result.Value.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Test]
    public async Task Create_ShouldReturnFields_WhenProvided()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto
            {
                DisplayName = "Email Address",
                Type = "String"
            },
            new FieldDefinitionDto
            {
                DisplayName = "Address",
                Type = "Object",
                Fields =
                [
                    new FieldDefinitionDto
                    {
                        DisplayName = "City",
                        Type = "String"
                    }
                ]
            }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Fields.Count.ShouldBe(2);
        result.Value.Fields[0].SchemaName.ShouldBe("emailAddress");
        result.Value.Fields[1].SchemaName.ShouldBe("address");
        result.Value.Fields[1].Fields.ShouldNotBeNull();
        result.Value.Fields[1].Fields!.Count.ShouldBe(1);
        result.Value.Fields[1].Fields![0].SchemaName.ShouldBe("city");
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenDisplayNameEmpty()
    {
        var request = NewCreateDto(displayName: "");

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenDisplayNameTooLong()
    {
        var request = NewCreateDto(displayName: new string('a', 129));

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenDisplayNameHasNoAlphanumeric()
    {
        var request = NewCreateDto(displayName: "!!!");

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenDescriptionTooLong()
    {
        var request = NewCreateDto(description: new string('a', 201));

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenFieldTypeInvalid()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto
            {
                DisplayName = "Bad",
                Type = "Invalid"
            }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn401_WhenNotAuthenticated()
    {
        _factory.FakeUser.IsAuthenticated = false;
        var request = NewCreateDto();

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(401);
    }

    #endregion

    #region GET /blueprints/entity-definitions

    [Test]
    public async Task GetList_ShouldReturnList()
    {
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto());
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto());

        var result = await _client.SafeGetEntityDefinitionsAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task GetList_ShouldReturn401_WhenNotAuthenticated()
    {
        _factory.FakeUser.IsAuthenticated = false;

        var result = await _client.SafeGetEntityDefinitionsAsync();

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(401);
    }

    #endregion

    #region GET /blueprints/entity-definitions/{id}

    [Test]
    public async Task GetById_ShouldReturnEntity_WhenExists()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: "GetById Target"))).Value;

        var result = await _client.SafeGetEntityDefinitionAsync(created.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(created.Id);
        result.Value.DisplayName.ShouldBe("GetById Target");
    }

    [Test]
    public async Task GetById_ShouldReturn404_WhenNotExists()
    {
        var result = await _client.SafeGetEntityDefinitionAsync(Guid.CreateVersion7());

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(404);
    }

    #endregion

    #region PUT /blueprints/entity-definitions/{id}

    [Test]
    public async Task Update_ShouldReturnUpdated_WhenValid()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: "Before Update"))).Value;

        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            DisplayName = created.DisplayName,
            Description = "Updated description",
            Fields =
            [
                new FieldDefinitionDto
                {
                    DisplayName = "New Field",
                    Type = "Number"
                }
            ],
            Tags = ["updated"]
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(created.Id);
        result.Value.Description.ShouldBe("Updated description");
        result.Value.Fields.Count.ShouldBe(1);
        result.Value.Fields[0].SchemaName.ShouldBe("newField");
        result.Value.Tags.ShouldContain("updated");
    }

    [Test]
    public async Task Update_ShouldReturn409_WhenETagMismatch()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag + 999,
            DisplayName = created.DisplayName,
            Description = "Should not apply"
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(409);
    }

    [Test]
    public async Task Update_ShouldReturn404_WhenNotExists()
    {
        var updateDto = new UpdateEntityDefinitionDto { DisplayName = "Nonexistent" };

        var result = await _client.SafeUpdateEntityDefinitionAsync(Guid.CreateVersion7(), updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(404);
    }

    [Test]
    public async Task Update_ShouldReturn400_WhenDescriptionTooLong()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            DisplayName = created.DisplayName,
            Description = new string('a', 201)
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    #endregion

    #region DELETE /blueprints/entity-definitions/{id}

    [Test]
    public async Task Delete_ShouldReturn200_WhenExists()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: "To Delete"))).Value;

        var result = await _client.SafeDeleteEntityDefinitionAsync(created.Id);

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task Delete_ShouldReturn404_WhenNotExists()
    {
        var result = await _client.SafeDeleteEntityDefinitionAsync(Guid.CreateVersion7());

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(404);
    }

    [Test]
    public async Task Delete_ThenGet_ShouldReturn404()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: "Ephemeral"))).Value;

        await _client.SafeDeleteEntityDefinitionAsync(created.Id);

        var result = await _client.SafeGetEntityDefinitionAsync(created.Id);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(404);
    }

    #endregion
}
