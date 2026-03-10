using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using FluentValidation.TestHelper;

namespace Dilcore.Blueprints.Contracts.Tests.EntityDefinitions.Create;

public class CreateEntityDefinitionDtoValidatorTests
{
    private CreateEntityDefinitionDtoValidator? _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateEntityDefinitionDtoValidator();
    }
}
