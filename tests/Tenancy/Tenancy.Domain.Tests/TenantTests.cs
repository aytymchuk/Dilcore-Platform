using Dilcore.Tenancy.Domain;
using Shouldly;

namespace Dilcore.Tenancy.Domain.Tests;

public class TenantTests
{
    [TestCase("valid-name")]
    [TestCase("name123")]
    [TestCase("multi-part-kebab-name")]
    [TestCase("a")]
    [TestCase("1")]
    public void SystemName_ValidInput_ShouldNotThrow(string validName)
    {
        // Act & Assert
        Should.NotThrow(() => new Tenant
        {
            SystemName = validName,
            Name = "Test",
            StoragePrefix = "test"
        });
    }

    [TestCase("Invalid Name")]
    [TestCase("invalid_name")]
    [TestCase("invalid.name")]
    [TestCase("InvalidName")]
    [TestCase("-start-with-hyphen")]
    [TestCase("end-with-hyphen-")]
    [TestCase("double--hyphen")]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void SystemName_InvalidInput_ShouldThrowArgumentException(string? invalidName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Tenant
        {
            SystemName = invalidName!,
            Name = "Test",
            StoragePrefix = "test"
        })
        .Message.ShouldContain("System name must be in lower-kebab-case format.");
    }

    [TestCase("My Tenant", "my-tenant")]
    [TestCase("Tenant #1!", "tenant-1")]
    [TestCase("  Spaces  ", "spaces")]
    [TestCase("Already-Kebab", "already-kebab")]
    [TestCase("Multiple   Spaces", "multiple-spaces")]
    [TestCase("Special@#$%Characters", "special-characters")]
    public void ToKebabCase_ShouldReturnExpectedResult(string input, string expected)
    {
        // Act
        var result = Tenant.ToKebabCase(input);

        // Assert
        result.ShouldBe(expected);
    }
}
