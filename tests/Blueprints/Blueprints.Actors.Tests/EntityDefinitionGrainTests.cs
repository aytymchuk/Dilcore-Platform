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
        FieldDefinitionGrainDto[]? fields = null,
        string[]? tags = null) => new()
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
    public async Task CreateAsync_ShouldReturnValidation_WhenDisplayNameHasNoAlphanumeric()
    {
        var grain = GetGrain();

        var result = await grain.CreateAsync(CreateCommand(displayName: "!!!"));

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.ValidationErrorCode);
        result.ErrorMessage.ShouldNotBeNullOrEmpty();
        result.ErrorMessage!.ShouldContain("alphanumeric");
        result.Entity.ShouldBeNull();
    }

    [Test]
    public async Task CreateAsync_ShouldReturnValidation_WhenEntitySchemaNameHasNoAlphanumeric()
    {
        var grain = GetGrain();

        var result = await grain.CreateAsync(CreateCommand(displayName: "Valid Name") with { SchemaName = "---" });

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.ValidationErrorCode);
        result.ErrorMessage!.ShouldContain("alphanumeric");
        result.Entity.ShouldBeNull();
    }

    [Test]
    public async Task CreateAsync_ShouldReturnValidation_WhenFieldSchemaNameHasNoAlphanumeric()
    {
        var grain = GetGrain();

        var result = await grain.CreateAsync(CreateCommand(
            displayName: "Valid Entity",
            fields: [new() { SchemaName = "---", DisplayName = "Some Field", Type = "String" }]));

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.ValidationErrorCode);
        result.ErrorMessage!.ShouldContain("alphanumeric");
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
        var fields = new FieldDefinitionGrainDto[]
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
        string[] tags = ["crm", "core"];

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
        entity.Fields.Length.ShouldBe(2);
        entity.Fields[0].SchemaName.ShouldBe("firstName");
        entity.Fields[1].SchemaName.ShouldBe("address");
        entity.Fields[1].Fields.ShouldNotBeNull();
        entity.Fields[1].Fields!.Length.ShouldBe(1);
        entity.Fields[1].Fields![0].SchemaName.ShouldBe("street");
        entity.Tags.Length.ShouldBe(2);
        entity.Tags.ShouldContain("crm");
        entity.Tags.ShouldContain("core");
    }

    [Test]
    public async Task CreateAsync_ShouldRejectDuplicateFieldSchemaNames()
    {
        var grain = GetGrain();

        var result = await grain.CreateAsync(CreateCommand(
            displayName: "DupCreateTest",
            fields:
            [
                new() { SchemaName = "", DisplayName = "Same Name", Type = "String" },
                new() { SchemaName = "", DisplayName = "Same Name", Type = "Number" }
            ]));

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.ValidationErrorCode);
        result.ErrorMessage!.ShouldContain("Duplicate field schema name");
    }

    [Test]
    public async Task CreateAsync_ShouldRejectDuplicateFieldSchemaNames_CaseInsensitive()
    {
        var grain = GetGrain();

        var result = await grain.CreateAsync(CreateCommand(
            displayName: "CaseInsensitiveDupTest",
            fields:
            [
                new() { SchemaName = "fieldA", DisplayName = "Field A", Type = "String" },
                new() { SchemaName = "FIELDA", DisplayName = "Field A Again", Type = "Number" }
            ]));

        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage!.ShouldContain("Duplicate field schema name 'FIELDA'");
    }

    [Test]
    public async Task CreateAsync_ShouldPreserveExplicitSchemaName()
    {
        var grain = GetGrain();
        var command = CreateCommand(displayName: "Ignore Me") with { SchemaName = "explicitName" };

        var result = await grain.CreateAsync(command);

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.SchemaName.ShouldBe("explicitName");
    }

    [Test]
    public async Task CreateAsync_ShouldPreserveExplicitFieldSchemaNames()
    {
        var grain = GetGrain();
        var result = await grain.CreateAsync(CreateCommand(
            displayName: "FieldPreserveTest",
            fields:
            [
                new() { SchemaName = "customField", DisplayName = "Some Display", Type = "String" }
            ]));

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.Fields[0].SchemaName.ShouldBe("customField");
    }

    [Test]
    public async Task CreateAsync_ShouldRejectReservedFieldSchemaNames()
    {
        var grain = GetGrain();

        var result = await grain.CreateAsync(CreateCommand(
            displayName: "ReservedTest",
            fields: [new() { SchemaName = "createdAt", DisplayName = "Created At", Type = "DateTime" }]));

        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage!.ShouldContain("Field schema name 'createdAt' is reserved.");
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
        var newFields = new FieldDefinitionGrainDto[]
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
            DisplayName = "New Name",
            Description = "Updated description",
            IsAbstract = true,
            Fields = newFields,
            Tags = ["updated"]
        });

        result.IsSuccess.ShouldBeTrue();
        var entity = result.Entity!;
        entity.Id.ShouldBe(createResult.Entity!.Id);
        entity.DisplayName.ShouldBe("New Name");
        entity.SchemaName.ShouldBe("oldName");
        entity.Description.ShouldBe("Updated description");
        entity.IsAbstract.ShouldBeTrue();
        entity.Fields.Length.ShouldBe(1);
        entity.Fields[0].SchemaName.ShouldBe("email");
        entity.Tags.ShouldContain("updated");
    }

    [Test]
    public async Task UpdateAsync_ShouldNotChangeSchemaName_EvenWhenDisplayNameChanges()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "Original"));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            DisplayName = "Completely Different Name",
            Description = "Changed description"
        });

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.DisplayName.ShouldBe("Completely Different Name");
        result.Entity!.SchemaName.ShouldBe("original");
    }

    [Test]
    public async Task UpdateAsync_ShouldPreserveDisplayName_WhenNotProvided()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "Keep This"));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Description = "Changed description"
        });

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.DisplayName.ShouldBe("Keep This");
        result.Entity!.SchemaName.ShouldBe("keepThis");
    }

    [Test]
    public async Task UpdateAsync_ShouldNotChangeExtendsEntityId()
    {
        var grain = GetGrain();
        var extendsId = Guid.CreateVersion7();
        var createResult = await grain.CreateAsync(CreateCommand(
            displayName: "Child Entity",
            extendsEntityId: extendsId));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Description = "Updated"
        });

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.ExtendsEntityId.ShouldBe(extendsId);
    }

    [Test]
    public async Task UpdateAsync_ShouldPreserveExistingFieldSchemaNames()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(
            displayName: "TestEntity",
            fields:
            [
                new() { SchemaName = "", DisplayName = "First Name", Type = "String" },
                new() { SchemaName = "", DisplayName = "Last Name", Type = "String" }
            ]));

        var created = createResult.Entity!;
        created.Fields[0].SchemaName.ShouldBe("firstName");
        created.Fields[1].SchemaName.ShouldBe("lastName");

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = created.ETag,
            Fields =
            [
                new() { SchemaName = "firstName", DisplayName = "Updated Display", Type = "String" },
                new() { SchemaName = "lastName", DisplayName = "Also Updated", Type = "String" },
                new() { SchemaName = "", DisplayName = "Email Address", Type = "String" }
            ]
        });

        result.IsSuccess.ShouldBeTrue();
        result.Entity!.Fields.Length.ShouldBe(3);
        result.Entity!.Fields[0].SchemaName.ShouldBe("firstName");
        result.Entity!.Fields[0].DisplayName.ShouldBe("Updated Display");
        result.Entity!.Fields[1].SchemaName.ShouldBe("lastName");
        result.Entity!.Fields[2].SchemaName.ShouldBe("emailAddress");
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

        updateResult.Entity!.UpdatedAt.ShouldBeGreaterThan(createResult.Entity!.UpdatedAt);
        updateResult.Entity.CreatedAt.ShouldBe(createResult.Entity.CreatedAt);
    }

    [Test]
    public async Task UpdateAsync_ShouldTrackAddedFields()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(
            displayName: "Tracked",
            fields:
            [
                new() { SchemaName = "", DisplayName = "Name", Type = "String" }
            ]));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields =
            [
                new() { SchemaName = "name", DisplayName = "Name", Type = "String" },
                new() { SchemaName = "", DisplayName = "Email", Type = "String" }
            ]
        });

        result.IsSuccess.ShouldBeTrue();
        result.AddedFields.ShouldNotBeNull();
        result.AddedFields.ShouldContain("email");
        result.RemovedFields.ShouldNotBeNull();
        result.RemovedFields.ShouldBeEmpty();
    }

    [Test]
    public async Task UpdateAsync_ShouldTrackRemovedFields()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(
            displayName: "Tracked",
            fields:
            [
                new() { SchemaName = "", DisplayName = "Name", Type = "String" },
                new() { SchemaName = "", DisplayName = "Phone", Type = "String" }
            ]));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields =
            [
                new() { SchemaName = "name", DisplayName = "Name", Type = "String" }
            ]
        });

        result.IsSuccess.ShouldBeTrue();
        result.RemovedFields.ShouldNotBeNull();
        result.RemovedFields.ShouldContain("phone");
        result.AddedFields.ShouldNotBeNull();
        result.AddedFields.ShouldBeEmpty();
    }

    [Test]
    public async Task UpdateAsync_ShouldTrackNoChanges_WhenFieldsAreSame()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(
            displayName: "Tracked",
            fields:
            [
                new() { SchemaName = "", DisplayName = "Name", Type = "String" }
            ]));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields =
            [
                new() { SchemaName = "name", DisplayName = "Name", Type = "String" }
            ]
        });

        result.IsSuccess.ShouldBeTrue();
        result.AddedFields.ShouldBeEmpty();
        result.RemovedFields.ShouldBeEmpty();
    }

    [Test]
    public async Task UpdateAsync_ShouldTrackMixedChanges()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(
            displayName: "Tracked",
            fields:
            [
                new() { SchemaName = "", DisplayName = "First Name", Type = "String" },
                new() { SchemaName = "", DisplayName = "Last Name", Type = "String" }
            ]));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields =
            [
                new() { SchemaName = "firstName", DisplayName = "First Name", Type = "String" },
                new() { SchemaName = "", DisplayName = "Email Address", Type = "String" }
            ]
        });

        result.IsSuccess.ShouldBeTrue();
        result.AddedFields!.ShouldContain("emailAddress");
        result.RemovedFields!.ShouldContain("lastName");
    }

    [Test]
    public async Task UpdateAsync_ShouldRejectDuplicateFieldSchemaNames()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "DupTest"));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields =
            [
                new() { SchemaName = "", DisplayName = "Same Name", Type = "String" },
                new() { SchemaName = "", DisplayName = "Same Name", Type = "Number" }
            ]
        });

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.ValidationErrorCode);
        result.ErrorMessage!.ShouldContain("Duplicate field schema name");
    }

    [Test]
    public async Task UpdateAsync_ShouldRejectDuplicateFieldSchemaNames_CaseInsensitive()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "CaseDupUpdate"));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields =
            [
                new() { SchemaName = "fieldX", DisplayName = "Field X", Type = "String" },
                new() { SchemaName = "FIELDX", DisplayName = "Field X Again", Type = "Number" }
            ]
        });

        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage!.ShouldContain("Duplicate field schema name 'FIELDX'");
    }

    [Test]
    public async Task UpdateAsync_ShouldRejectNestedDuplicateFieldSchemaNames()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "NestedDupUpdate"));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields =
            [
                new()
                {
                    SchemaName = "root",
                    DisplayName = "Root",
                    Type = "Object",
                    Fields = [new() { SchemaName = "root", DisplayName = "Sub Root", Type = "String" }]
                }
            ]
        });

        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage!.ShouldContain("Duplicate field schema name 'root'");
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnValidation_WhenFieldSchemaNameHasNoAlphanumeric()
    {
        var grain = GetGrain();
        var createResult = await grain.CreateAsync(CreateCommand(displayName: "UpdateSchemaTest"));

        var result = await grain.UpdateAsync(new UpdateEntityDefinitionGrainCommand
        {
            ETag = createResult.Entity!.ETag,
            Fields = [new() { SchemaName = "---", DisplayName = "Invalid Schema", Type = "String" }]
        });

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(EntityDefinitionGrainResult.ValidationErrorCode);
        result.ErrorMessage!.ShouldContain("alphanumeric");
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
