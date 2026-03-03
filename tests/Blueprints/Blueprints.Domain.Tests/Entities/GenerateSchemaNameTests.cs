using Shouldly;

namespace Dilcore.Blueprints.Domain.Tests.Entities;

[TestFixture]
public class GenerateSchemaNameTests
{
    [TestCase("Customer Profile", "customerProfile")]
    [TestCase("Invoice Record", "invoiceRecord")]
    [TestCase("Order Line Item", "orderLineItem")]
    [TestCase("simple", "simple")]
    public void ShouldConvertToCamelCase(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("  Leading Spaces  ", "leadingSpaces")]
    [TestCase("  hello  ", "hello")]
    public void ShouldTrimWhitespace(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("Special!@#Chars$%^", "specialChars")]
    [TestCase("hello@world.com", "helloWorldCom")]
    [TestCase("price ($)", "price")]
    [TestCase("100% done", "100Done")]
    [TestCase("a&b", "aB")]
    public void ShouldSplitOnSpecialCharsAndCamelCase(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("Multiple   Spaces", "multipleSpaces")]
    [TestCase("a    b", "aB")]
    [TestCase("one ! two", "oneTwo")]
    public void ShouldHandleMultipleDelimiters(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("Already-Hyphenated", "alreadyHyphenated")]
    [TestCase("kebab-case-name", "kebabCaseName")]
    [TestCase("a-b-c", "aBC")]
    public void ShouldTreatHyphensAsWordSeparators(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("-leading", "leading")]
    [TestCase("trailing-", "trailing")]
    [TestCase("-both-", "both")]
    [TestCase("---surrounded---", "surrounded")]
    public void ShouldIgnoreLeadingAndTrailingDelimiters(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("Mixed CASE Name", "mixedCaseName")]
    [TestCase("ALL UPPER", "allUpper")]
    [TestCase("HTMLParser", "htmlparser")]
    public void ShouldLowercaseFirstWordAndCapitalizeSubsequent(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("dots.and_underscores", "dotsAndUnderscores")]
    [TestCase("snake_case", "snakeCase")]
    [TestCase("dot.notation", "dotNotation")]
    public void ShouldTreatSeparatorsAsWordBoundaries(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("abc123", "abc123")]
    [TestCase("item-42", "item42")]
    [TestCase("v2 Release", "v2Release")]
    public void ShouldPreserveDigits(string input, string expected)
    {
        SchemaNameGenerator.Generate(input).ShouldBe(expected);
    }

    [TestCase("!!!")]
    [TestCase("@#$%")]
    public void ShouldReturnEmptyForNonAlphanumericInput(string input)
    {
        SchemaNameGenerator.Generate(input).ShouldBeEmpty();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ShouldThrowForNullOrWhitespaceInput(string? input)
    {
        Should.Throw<ArgumentException>(() => SchemaNameGenerator.Generate(input!));
    }
}
