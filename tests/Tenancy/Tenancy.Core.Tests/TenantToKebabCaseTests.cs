using Dilcore.Tenancy.Domain;
using Shouldly;

namespace Dilcore.Tenancy.Core.Tests;

public class TenantToKebabCaseTests
{
    [TestCase("MyTenant", "mytenant")]
    [TestCase("My Tenant", "my-tenant")]
    [TestCase("My  Tenant", "my-tenant")]
    [TestCase("Tenant #1!", "tenant-1")]
    [TestCase("  Tenant  ", "tenant")]
    [TestCase("already-kebab", "already-kebab")]
    [TestCase("TenantWith123Numbers", "tenantwith123numbers")]
    [TestCase("Tenant-With-Hyphens", "tenant-with-hyphens")]
    [TestCase("Tenant_With_Underscore", "tenant-with-underscore")]
    [TestCase("Mixed-CASE-Input", "mixed-case-input")]
    [TestCase("Simple", "simple")]
    public void ToKebabCase_ValidInput_ReturnsExpectedResult(string input, string expected)
    {
        // Act
        var result = Tenant.ToKebabCase(input);

        // Assert
        result.ShouldBe(expected);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ToKebabCase_NullOrEmptyInput_ReturnsEmptyString(string? input)
    {
        // Act
        var result = Tenant.ToKebabCase(input!);

        // Assert
        result.ShouldBe(string.Empty);
    }
}
