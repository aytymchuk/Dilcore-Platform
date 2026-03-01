using Dilcore.Blueprints.Actors.Abstractions;
using Orleans.TestingHost;
using Shouldly;

namespace Dilcore.Blueprints.Actors.Tests;

[TestFixture]
[NonParallelizable]
public class EntityDefinitionGrainTests
{
    private ClusterFixture _fixture = null!;
    private TestCluster Cluster => _fixture.Cluster;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _fixture = new ClusterFixture();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _fixture.Dispose();
    }

    private IEntityDefinitionGrain GetGrain() =>
        Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(Guid.CreateVersion7());

    private static CreateEntityDefinitionGrainCommand CreateCommand(
        string displayName = "Customer Profile",
        string? description = "Stores customer data",
        bool isAbstract = false,
        Guid? extendsEntityId = null,
        List<FieldDefinitionGrainDto>? fields = null,
        List<string>? tags = null) => new()
    {
        DisplayName = displayName,
        Description = description,
        IsAbstract = isAbstract,
        ExtendsEntityId = extendsEntityId,
        Fields = fields ?? [],
        Tags = tags ?? []
    };

    #region CreateAsync

    [Test]
    public async Task CreateAsync_ShouldSucceed_WhenGrainIsNew()
    {
        var grain = GetGrain();
        var command = CreateCommand();

        var result = await grain.CreateAsync(command);

        result.IsSuccess.ShouldBeTrue();
        result.Entity.ShouldNotBeNull();
        result.Entity.DisplayName.ShouldBe("Customer Profile");
        result.Entity.SchemaName.ShouldBe("customerProfile");
        result.Entity.Description.ShouldBe("Stores customer data");
        result.Entity.IsAbstract.ShouldBeFalse();
        result.Entity.ExtendsEntityId.ShouldBeNull();
        result.Entity.Fields.ShouldBeEmpty();
        result.Entity.Tags.ShouldBeEmpty();
        result.Entity.Id.ShouldNotBe(Guid.Empty);
        result.Entity.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        result.Entity.UpdatedAt.ShouldBe(result.Entity.CreatedAt);
    }

    [Test]
    public async Task CreateAsync_ShouldFail_WhenAlreadyExists()
    {
        var grain = GetGrain();
        await grain.CreateAsync(CreateCommand());

        var result = await grain.CreateAsync(CreateCommand(displayName: "Another Name"));

        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNullOrEmpty();
        result.ErrorMessage!.ShouldContain("already exists");
        result.Entity.ShouldBeNull();
    }

    [Test]
    public async Task CreateAsync_ShouldGenerateSchemaName_FromDisplayName()
    {
        var grain = GetGrain();

        var result = await grain.CreateAsync(CreateCommand(displayName: "  Order  Line Item! "));

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.SchemaName.ShouldBe("orderLineItem");
    }

    [Test]
    public async Task CreateAsync_ShouldPreserveAllProperties()
    {
        var grain = GetGrain();
        var extendsId = Guid.CreateVersion7();
        var fields = new List<FieldDefinitionGrainDto>
        {
            new()
            {
                SchemaName = "",
                DisplayName = "First Name",
                Type = "String"
            },
            new()
            {
                SchemaName = "",
                DisplayName = "Address",
                Type = "Object",
                Fields =
                [
                    new FieldDefinitionGrainDto
                    {
                        SchemaName = "",
                        DisplayName = "Street",
                        Type = "String"
                    }
                ]
            }
        };
        List<string> tags = ["crm", "core"];

        var result = await grain.CreateAsync(CreateCommand(
            displayName: "Contact",
            description: "A contact entity",
            isAbstract: true,
            extendsEntityId: extendsId,
            fields: fields,
            tags: tags));

        result.IsSuccess.ShouldBeTrue();
        var entity = result.Entity!;
        entity.IsAbstract.ShouldBeTrue();
        entity.ExtendsEntityId.ShouldBe(extendsId);
        entity.Fields.Count.ShouldBe(2);
        entity.Fields[0].SchemaName.ShouldBe("firstName");
        entity.Fields[1].SchemaName.ShouldBe("address");
        entity.Fields[1].Fields.ShouldNotBeNull();
        entity.Fields[1].Fields!.Count.ShouldBe(1);
        entity.Fields[1].Fields![0].SchemaName.ShouldBe("street");
        entity.Tags.Count.ShouldBe(2);
        entity.Tags.ShouldContain("crm");
        entity.Tags.ShouldContain("core");
    }

    #endregion

    #region GetAsync

    [Test]
    public async Task GetAsync_ShouldReturnNull_WhenNotExists()
    {
        var grain = GetGrain();

        var result = await grain.GetAsync();

        result.ShouldBeNull();
    }

    [Test]
    public async Task GetAsync_ShouldReturnDto_WhenExists()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "Invoice"));

        var result = await grain.GetAsync();

        result.ShouldNotBeNull();
        result.Id.ShouldBe(createResult.Entity!.Id);
        result.DisplayName.ShouldBe("Invoice");
        result.SchemaName.ShouldBe("invoice");
    }

    [Test]
    public async Task GetAsync_ShouldPersistState_AcrossGrainReferences()
    {
        var grainId = Guid.CreateVersion7();
        var grain = Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(grainId);
        await grain.CreateAsync(CreateCommand(displayName: "Product"));

        var newRef = Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(grainId);
        var result = await newRef.GetAsync();

        result.ShouldNotBeNull();
        result.DisplayName.ShouldBe("Product");
    }

    #endregion

    #region UpdateAsync

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenNotExists()
    {
        var grain = GetGrain();

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            Description = "Anything"
        });

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.NotFoundCode);
        result.ErrorMessage!.ShouldContain("does not exist");
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenETagMismatch()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand());

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag + 999,
            Description = "Should not apply"
        });

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.ETagMismatchCode);
        result.ErrorMessage!.ShouldContain("ETag mismatch");
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateAllProperties()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "Old Name"));
        var newExtendsId = Guid.CreateVersion7();
        var newFields = new List<FieldDefinitionGrainDto>
        {
            new()
            {
                SchemaName = "",
                DisplayName = "Email",
                Type = "String"
            }
        };

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Description = "Updated description",
            IsAbstract = true,
            ExtendsEntityId = newExtendsId,
            Fields = newFields,
            Tags = ["updated"]
        });

        result.IsSuccess.ShouldBeTrue();
        var entity = result.Entity!;
        entity.Id.ShouldBe(createResult.Entity!.Id);
        entity.DisplayName.ShouldBe("Old Name");
        entity.SchemaName.ShouldBe("oldName");
        entity.Description.ShouldBe("Updated description");
        entity.IsAbstract.ShouldBeTrue();
        entity.ExtendsEntityId.ShouldBe(newExtendsId);
        entity.Fields.Count.ShouldBe(1);
        entity.Fields[0].SchemaName.ShouldBe("email");
        entity.Tags.ShouldContain("updated");
    }

    [Test]
    public async Task UpdateAsync_ShouldNotChangeDisplayNameOrSchemaName()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "Original"));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Description = "Changed description"
        });

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.DisplayName.ShouldBe("Original");
        result.Entity!.SchemaName.ShouldBe("original");
    }

    [Test]
    public async Task UpdateAsync_ShouldAdvanceUpdatedAt()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand());

        await Task.Delay(50);

        var updateResult = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Description = "Updated"
        });

        updateResult.Entity!.UpdatedAt.ShouldBeGreaterThanOrEqualTo(createResult.Entity!.UpdatedAt);
        updateResult.Entity.CreatedAt.ShouldBe(createResult.Entity.CreatedAt);
    }

    #endregion

    #region DeleteAsync

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenNotExists()
    {
        var grain = GetGrain();

        var result = await grain.DeleteAsync();

        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage!.ShouldContain("does not exist");
    }

    [Test]
    public async Task DeleteAsync_ShouldSucceed_WhenExists()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "ToDelete"));

        var result = await grain.DeleteAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Entity.ShouldNotBeNull();
        result.Entity.Id.ShouldBe(createResult.Entity!.Id);
        result.Entity.DisplayName.ShouldBe("ToDelete");
    }

    [Test]
    public async Task DeleteAsync_ShouldClearState_SoGetReturnsNull()
    {
        var grainId = Guid.CreateVersion7();
        var grain = Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(grainId);
        await grain.CreateAsync(CreateCommand(displayName: "Ephemeral"));

        await grain.DeleteAsync();

        var freshRef = Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(grainId);
        var result = await freshRef.GetAsync();
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteAsync_ShouldAllowRecreation_AfterDeletion()
    {
        var grainId = Guid.CreateVersion7();
        var grain = Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(grainId);
        await grain.CreateAsync(CreateCommand(displayName: "First"));
        await grain.DeleteAsync();

        var freshGrain = Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(grainId);
        var result = await freshGrain.CreateAsync(CreateCommand(displayName: "Second"));

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.DisplayName.ShouldBe("Second");
    }

    #endregion
}
