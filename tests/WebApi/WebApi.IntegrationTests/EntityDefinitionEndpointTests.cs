using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Update;
using Dilcore.Blueprints.Domain;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using Dilcore.WebApi.Client.Extensions;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
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
        List<FieldDefinitionDto>? fields = null,
        bool isAbstract = false,
        Guid? extendsEntityId = null,
        List<string>? tags = null,
        string? schemaName = null) => new()
    {
        DisplayName = displayName ?? $"Entity {Guid.CreateVersion7():N}",
        Description = description ?? "Test entity",
        Fields = fields ?? [],
        IsAbstract = isAbstract,
        ExtendsEntityId = extendsEntityId,
        Tags = tags ?? [],
        SchemaName = schemaName
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
    public async Task Create_ShouldReturn201_WhenExtendsExistingEntity()
    {
        var parentRequest = NewCreateDto(displayName: $"ParentEntity {Guid.CreateVersion7():N}");
        var parentResult = await _client.SafeCreateEntityDefinitionAsync(parentRequest);
        parentResult.IsSuccess.ShouldBeTrue();

        var childRequest = new CreateEntityDefinitionDto
        {
            DisplayName = $"ChildEntity {Guid.CreateVersion7():N}",
            Description = "Extends parent",
            ExtendsEntityId = parentResult.Value.Id
        };

        var result = await _client.SafeCreateEntityDefinitionAsync(childRequest);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ExtendsEntityId.ShouldBe(parentResult.Value.Id);
    }

    [Test]
    public async Task Create_ShouldFail_WhenExtendsNonExistentEntity()
    {
        var request = new CreateEntityDefinitionDto
        {
            DisplayName = $"Orphan {Guid.CreateVersion7():N}",
            Description = "References nothing",
            ExtendsEntityId = Guid.CreateVersion7()
        };

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

    [Test]
    public async Task Create_ShouldReturn409_WhenDuplicateDisplayName()
    {
        var displayName = $"Unique Entity {Guid.CreateVersion7():N}";
        var first = await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(displayName: displayName));
        first.IsSuccess.ShouldBeTrue();

        var result = await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(displayName: displayName));

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(409);
    }

    [Test]
    public async Task Create_ShouldReturn409_WhenExplicitSchemaNameAlreadyExists()
    {
        var schemaName = $"customSchema{Guid.CreateVersion7():N}";
        var first = await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(schemaName: schemaName));
        first.IsSuccess.ShouldBeTrue();

        var result = await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(schemaName: schemaName));

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(409);
    }

    [Test]
    public async Task Create_ShouldReturn201_WithCustomSchemaName()
    {
        var suffix = Guid.CreateVersion7().ToString("N");
        var request = NewCreateDto(schemaName: $"my-custom-schema-{suffix}");

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.SchemaName.ShouldStartWith("myCustomSchema");
        result.Value.SchemaName.ShouldNotBe(
            SchemaNameGenerator.Generate(request.DisplayName),
            "Schema name should derive from the explicit SchemaName, not DisplayName");
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenObjectFieldHasNoNestedFields()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto { DisplayName = "Details", Type = "Object" }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenArrayFieldHasNoNestedFields()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto { DisplayName = "Items", Type = "Array" }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenPrimitiveFieldHasNestedFields()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto
            {
                DisplayName = "Name",
                Type = "String",
                Fields = [new FieldDefinitionDto { DisplayName = "Sub", Type = "String" }]
            }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenNestingDepthExceeded()
    {
        var deepField = new FieldDefinitionDto { DisplayName = "Leaf", Type = "String" };
        for (var i = 0; i < 5; i++)
        {
            deepField = new FieldDefinitionDto
            {
                DisplayName = $"Level{i}",
                Type = "Object",
                Fields = [deepField]
            };
        }

        var request = NewCreateDto(fields: [deepField]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenTooManyTopLevelFields()
    {
        var fields = Enumerable.Range(0, 101)
            .Select(i => new FieldDefinitionDto { DisplayName = $"Field{i}xx", Type = "String" })
            .ToList();

        var request = NewCreateDto(fields: fields);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenTooManyNestedFields()
    {
        var nestedFields = Enumerable.Range(0, 51)
            .Select(i => new FieldDefinitionDto { DisplayName = $"Nested{i}xx", Type = "String" })
            .ToList();

        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto
            {
                DisplayName = "Parent",
                Type = "Object",
                Fields = nestedFields
            }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenFieldSchemaNameIsReserved()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto { DisplayName = "Id", Type = "String" }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenDuplicateFieldSchemaNames()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto { DisplayName = "Full Name", Type = "String" },
            new FieldDefinitionDto { DisplayName = "Full Name", Type = "Number" }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn201_WithAllPrimitiveFieldTypes()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto { DisplayName = "Text Field", Type = "String" },
            new FieldDefinitionDto { DisplayName = "Number Field", Type = "Number" },
            new FieldDefinitionDto { DisplayName = "Bool Field", Type = "Boolean" },
            new FieldDefinitionDto { DisplayName = "Date Field", Type = "DateTime" },
            new FieldDefinitionDto { DisplayName = "File Field", Type = "File" },
            new FieldDefinitionDto { DisplayName = "Id Field", Type = "Identifier" }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Fields.Count.ShouldBe(6);
        result.Value.Fields.Select(f => f.Type).ShouldBe(
            ["String", "Number", "Boolean", "DateTime", "File", "Identifier"]);
    }

    [Test]
    public async Task Create_ShouldReturn201_WithArrayFieldType()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto
            {
                DisplayName = "Tags List",
                Type = "Array",
                Fields =
                [
                    new FieldDefinitionDto { DisplayName = "Tag Value", Type = "String" }
                ]
            }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Fields.Count.ShouldBe(1);
        result.Value.Fields[0].Type.ShouldBe("Array");
        result.Value.Fields[0].SchemaName.ShouldBe("tagsList");
        result.Value.Fields[0].Fields.ShouldNotBeNull();
        result.Value.Fields[0].Fields!.Count.ShouldBe(1);
    }

    [Test]
    public async Task Create_ShouldReturn201_WithDeeplyNestedComplexFields()
    {
        var request = NewCreateDto(fields:
        [
            new FieldDefinitionDto
            {
                DisplayName = "Root Object",
                Type = "Object",
                Fields =
                [
                    new FieldDefinitionDto
                    {
                        DisplayName = "Items Array",
                        Type = "Array",
                        Fields =
                        [
                            new FieldDefinitionDto
                            {
                                DisplayName = "Item Detail",
                                Type = "Object",
                                Fields =
                                [
                                    new FieldDefinitionDto { DisplayName = "Item Name", Type = "String" }
                                ]
                            }
                        ]
                    }
                ]
            }
        ]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        var root = result.Value.Fields[0];
        root.Type.ShouldBe("Object");
        root.Fields![0].Type.ShouldBe("Array");
        root.Fields![0].Fields![0].Type.ShouldBe("Object");
        root.Fields![0].Fields![0].Fields![0].Type.ShouldBe("String");
    }

    [Test]
    public async Task Create_ShouldReturn201_WithIsAbstractTrue()
    {
        var request = NewCreateDto(
            displayName: $"Abstract Entity {Guid.CreateVersion7():N}",
            isAbstract: true);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsAbstract.ShouldBeTrue();
    }

    [Test]
    public async Task Create_ShouldReturn201_WithTags()
    {
        var request = NewCreateDto(tags: ["crm", "billing", "v2"]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Tags.Count.ShouldBe(3);
        result.Value.Tags.ShouldContain("crm");
        result.Value.Tags.ShouldContain("billing");
        result.Value.Tags.ShouldContain("v2");
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenTooManyTags()
    {
        var tags = Enumerable.Range(0, 21).Select(i => $"tag{i}").ToList();
        var request = NewCreateDto(tags: tags);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenTagTooLong()
    {
        var request = NewCreateDto(tags: [new string('a', 65)]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Create_ShouldReturn400_WhenTagHasInvalidFormat()
    {
        var request = NewCreateDto(tags: ["invalid tag with spaces"]);

        var result = await _client.SafeCreateEntityDefinitionAsync(request);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    #endregion

    #region GET /blueprints/entity-definitions

    [Test]
    public async Task GetList_ShouldReturnPagedList()
    {
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto());
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto());

        var result = await _client.SafeGetEntityDefinitionsAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.Value.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task GetList_ShouldReturn401_WhenNotAuthenticated()
    {
        _factory.FakeUser.IsAuthenticated = false;

        var result = await _client.SafeGetEntityDefinitionsAsync();

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(401);
    }

    [Test]
    public async Task GetList_ShouldRespectPagination()
    {
        var tag = $"page-{Guid.CreateVersion7():N}";
        for (var i = 0; i < 3; i++)
            await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(tags: [tag]));

        var result = await _client.SafeGetEntityDefinitionsAsync(skip: 0, take: 2, tags: tag);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task GetList_ShouldReturnEmpty_WhenSkipBeyondData()
    {
        var tag = $"skiptest-{Guid.CreateVersion7():N}";
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(tags: [tag]));

        var result = await _client.SafeGetEntityDefinitionsAsync(skip: 1000, take: 10, tags: tag);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(0);
        result.Value.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task GetList_ShouldFilterBySearch()
    {
        var marker = $"SearchTarget{Guid.CreateVersion7():N}";
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(displayName: marker));
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(displayName: "Unrelated Entity zz"));

        var result = await _client.SafeGetEntityDefinitionsAsync(search: marker);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
        result.Value.Items.ShouldAllBe(e => e.DisplayName.Contains(marker));
    }

    [Test]
    public async Task GetList_ShouldFilterByIsAbstract()
    {
        var tag = $"abstract-filter-{Guid.CreateVersion7():N}";
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(isAbstract: true, tags: [tag]));
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(isAbstract: false, tags: [tag]));

        var result = await _client.SafeGetEntityDefinitionsAsync(isAbstract: true, tags: tag);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldAllBe(e => e.IsAbstract);
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task GetList_ShouldFilterByTags()
    {
        var uniqueTag = $"tagfilter-{Guid.CreateVersion7():N}";
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(tags: [uniqueTag, "common"]));
        await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(tags: ["other"]));

        var result = await _client.SafeGetEntityDefinitionsAsync(tags: uniqueTag);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
        result.Value.Items.ShouldAllBe(e => e.Tags.Contains(uniqueTag));
    }

    [Test]
    public async Task GetList_ShouldReturnEmpty_WhenNoMatch()
    {
        var result = await _client.SafeGetEntityDefinitionsAsync(
            search: $"NonExistent{Guid.CreateVersion7():N}");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(0);
        result.Value.TotalCount.ShouldBe(0);
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
            DisplayName = "After Update",
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
        result.Value.DisplayName.ShouldBe("After Update");
        result.Value.SchemaName.ShouldBe("beforeUpdate");
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
            Description = "Should not apply"
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(409);
    }

    [Test]
    public async Task Update_ShouldReturn404_WhenNotExists()
    {
        var updateDto = new UpdateEntityDefinitionDto { Description = "Nonexistent" };

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
            Description = new string('a', 201)
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Update_ShouldPreserveExistingFieldSchemaNames()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(fields:
        [
            new FieldDefinitionDto { DisplayName = "Email Address", Type = "String" }
        ]))).Value;

        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            Fields =
            [
                new FieldDefinitionDto
                {
                    SchemaName = "emailAddress",
                    DisplayName = "Email Address Renamed",
                    Type = "String"
                },
                new FieldDefinitionDto { DisplayName = "Phone Number", Type = "String" }
            ]
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Fields.Count.ShouldBe(2);
        result.Value.Fields[0].SchemaName.ShouldBe("emailAddress");
        result.Value.Fields[1].SchemaName.ShouldBe("phoneNumber");
    }

    [Test]
    public async Task Update_ShouldRemoveFields_WhenOmittedFromUpdate()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto(fields:
        [
            new FieldDefinitionDto { DisplayName = "Keep Me", Type = "String" },
            new FieldDefinitionDto { DisplayName = "Remove Me", Type = "Number" }
        ]))).Value;
        created.Fields.Count.ShouldBe(2);

        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            Fields =
            [
                new FieldDefinitionDto
                {
                    SchemaName = "keepMe",
                    DisplayName = "Keep Me",
                    Type = "String"
                }
            ]
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Fields.Count.ShouldBe(1);
        result.Value.Fields[0].SchemaName.ShouldBe("keepMe");
    }

    [Test]
    public async Task Update_ShouldReturn400_WhenReservedFieldName()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            Fields =
            [
                new FieldDefinitionDto { DisplayName = "Type", Type = "String" }
            ]
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Update_ShouldReturn400_WhenDuplicateFieldNames()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            Fields =
            [
                new FieldDefinitionDto { DisplayName = "Status", Type = "String" },
                new FieldDefinitionDto { DisplayName = "Status", Type = "Number" }
            ]
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Update_ShouldReturn400_WhenObjectFieldHasNoNestedFields()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            Fields =
            [
                new FieldDefinitionDto { DisplayName = "Details", Type = "Object" }
            ]
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Update_ShouldReturn400_WhenDisplayNameTooLong()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            DisplayName = new string('a', 129)
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Update_ShouldReturn400_WhenDisplayNameHasNoAlphanumeric()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            DisplayName = "!!!"
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Update_ShouldReturn400_WhenFieldTypeInvalid()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;
        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            Fields =
            [
                new FieldDefinitionDto { DisplayName = "Bad Field", Type = "Invalid" }
            ]
        };

        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsFailed.ShouldBeTrue();
        GetStatusCode(result).ShouldBe(400);
    }

    [Test]
    public async Task Update_ShouldSupportSequentialETagChaining()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(NewCreateDto())).Value;

        var firstUpdate = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            Description = "First update"
        };
        var firstResult = await _client.SafeUpdateEntityDefinitionAsync(created.Id, firstUpdate);
        firstResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.ETag.ShouldNotBe(created.ETag);

        var secondUpdate = new UpdateEntityDefinitionDto
        {
            ETag = firstResult.Value.ETag,
            Description = "Second update"
        };
        var secondResult = await _client.SafeUpdateEntityDefinitionAsync(created.Id, secondUpdate);

        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.Description.ShouldBe("Second update");
        secondResult.Value.ETag.ShouldNotBe(firstResult.Value.ETag);
    }

    [Test]
    public async Task Update_ShouldNotChangeSchemaName_WhenDisplayNameChanges()
    {
        var created = (await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: "Original Name For Schema"))).Value;
        var originalSchema = created.SchemaName;

        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = created.ETag,
            DisplayName = "Completely Different Name"
        };
        var result = await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto);

        result.IsSuccess.ShouldBeTrue();
        result.Value.DisplayName.ShouldBe("Completely Different Name");
        result.Value.SchemaName.ShouldBe(originalSchema);
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

    [Test]
    public async Task Delete_ThenGetList_ShouldNotContainDeletedEntity()
    {
        var tag = $"dellist-{Guid.CreateVersion7():N}";
        var created = (await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(tags: [tag]))).Value;

        await _client.SafeDeleteEntityDefinitionAsync(created.Id);

        var result = await _client.SafeGetEntityDefinitionsAsync(tags: tag);
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotContain(e => e.Id == created.Id);
    }

    [Test]
    public async Task Delete_ThenCreateWithSameName_ShouldSucceed()
    {
        var displayName = $"Recyclable {Guid.CreateVersion7():N}";
        var created = (await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: displayName))).Value;

        await _client.SafeDeleteEntityDefinitionAsync(created.Id);

        var result = await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: displayName));

        result.IsSuccess.ShouldBeTrue();
        result.Value.DisplayName.ShouldBe(displayName);
    }

    #endregion

    #region Lifecycle

    [Test]
    public async Task FullLifecycle_CreateGetUpdateGetDeleteGet()
    {
        var createDto = NewCreateDto(
            displayName: $"Lifecycle Entity {Guid.CreateVersion7():N}",
            description: "Initial description",
            fields: [new FieldDefinitionDto { DisplayName = "Name", Type = "String" }],
            tags: ["lifecycle"]);

        var created = (await _client.SafeCreateEntityDefinitionAsync(createDto)).Value;
        created.Id.ShouldNotBe(Guid.Empty);

        var fetched = (await _client.SafeGetEntityDefinitionAsync(created.Id)).Value;
        fetched.DisplayName.ShouldBe(created.DisplayName);
        fetched.Fields.Count.ShouldBe(1);

        var updateDto = new UpdateEntityDefinitionDto
        {
            ETag = fetched.ETag,
            DisplayName = $"Updated Lifecycle {Guid.CreateVersion7():N}",
            Description = "Updated description",
            Fields =
            [
                new FieldDefinitionDto
                {
                    SchemaName = "name",
                    DisplayName = "Name",
                    Type = "String"
                },
                new FieldDefinitionDto { DisplayName = "Age", Type = "Number" }
            ],
            Tags = ["lifecycle", "updated"]
        };
        var updated = (await _client.SafeUpdateEntityDefinitionAsync(created.Id, updateDto)).Value;
        updated.ETag.ShouldNotBe(fetched.ETag);
        updated.Description.ShouldBe("Updated description");
        updated.Fields.Count.ShouldBe(2);
        updated.SchemaName.ShouldBe(created.SchemaName);

        var refetched = (await _client.SafeGetEntityDefinitionAsync(created.Id)).Value;
        refetched.Description.ShouldBe("Updated description");
        refetched.Fields.Count.ShouldBe(2);
        refetched.Tags.ShouldContain("updated");

        var deleteResult = await _client.SafeDeleteEntityDefinitionAsync(created.Id);
        deleteResult.IsSuccess.ShouldBeTrue();

        var afterDelete = await _client.SafeGetEntityDefinitionAsync(created.Id);
        afterDelete.IsFailed.ShouldBeTrue();
        GetStatusCode(afterDelete).ShouldBe(404);
    }

    #endregion

    #region Extends Entity

    [Test]
    public async Task Create_ShouldReturn201_WhenExtendsAbstractParent()
    {
        var parentResult = await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(
                displayName: $"Abstract Parent {Guid.CreateVersion7():N}",
                isAbstract: true));
        parentResult.IsSuccess.ShouldBeTrue();

        var childResult = await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(
                displayName: $"Concrete Child {Guid.CreateVersion7():N}",
                extendsEntityId: parentResult.Value.Id));

        childResult.IsSuccess.ShouldBeTrue();
        childResult.Value.ExtendsEntityId.ShouldBe(parentResult.Value.Id);
        childResult.Value.IsAbstract.ShouldBeFalse();
    }

    [Test]
    public async Task Delete_Parent_ShouldNotAffectChild()
    {
        var parentResult = await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(displayName: $"Parent To Delete {Guid.CreateVersion7():N}"));
        parentResult.IsSuccess.ShouldBeTrue();

        var childResult = await _client.SafeCreateEntityDefinitionAsync(
            NewCreateDto(
                displayName: $"Orphaned Child {Guid.CreateVersion7():N}",
                extendsEntityId: parentResult.Value.Id));
        childResult.IsSuccess.ShouldBeTrue();

        await _client.SafeDeleteEntityDefinitionAsync(parentResult.Value.Id);

        var childFetch = await _client.SafeGetEntityDefinitionAsync(childResult.Value.Id);
        childFetch.IsSuccess.ShouldBeTrue();
        childFetch.Value.ExtendsEntityId.ShouldBe(parentResult.Value.Id);
    }

    #endregion
}
