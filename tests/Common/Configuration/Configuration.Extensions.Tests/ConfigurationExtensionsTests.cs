using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Dilcore.Configuration.Extensions.Tests;

[TestFixture]
public class ConfigurationExtensionsTests
{
    [Test]
    public void RegisterConfiguration_Should_Bind_Configuration_To_Options()
    {
        // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"TestConfig:Value", "SomeValue"},
                {"TestConfig:Number", "42"}
            });
        var configuration = configBuilder.Build();

        // Act
        services.RegisterConfiguration<TestConfig>(configuration);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<TestConfig>>().Value;

        // Assert
        options.Value.ShouldBe("SomeValue");
        options.Number.ShouldBe(42);
    }



    [Test]
    public void GetSettings_Should_Return_Default_When_Section_Missing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = configuration.GetSettings<TestConfig>();

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(string.Empty);
    }

    [Test]
    public void GetRequiredSettings_Should_Return_Settings_When_Present()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"TestConfig:Value", "RequiredValue"}
            });
        var configuration = configBuilder.Build();

        // Act
        var result = configuration.GetRequiredSettings<TestConfig>();

        // Assert
        result.Value.ShouldBe("RequiredValue");
    }

    [Test]
    public void GetRequiredSettings_Should_Throw_When_Section_Missing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => configuration.GetRequiredSettings<TestConfig>())
            .Message.ShouldContain("Required configuration section 'TestConfig' is missing");
    }

    private class TestConfig
    {
        public string Value { get; set; } = string.Empty;
        public int Number { get; set; }
    }
}