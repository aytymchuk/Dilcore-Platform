using Dilcore.WebApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Dilcore.WebApi.Tests.Extensions;

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
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<TestConfig>>().Value;

        // Assert
        options.Value.ShouldBe("SomeValue");
        options.Number.ShouldBe(42);
    }
    
    [Test]
    public void RegisterConfiguration_With_SectionName_Should_Bind_Correctly()
    {
         // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"CustomSection:Value", "CustomValue"}
            });
        var configuration = configBuilder.Build();

         // Act
        services.RegisterConfiguration<TestConfig>(configuration, "CustomSection");
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<TestConfig>>().Value;

        // Assert
        options.Value.ShouldBe("CustomValue");
    }

    private class TestConfig
    {
        public string Value { get; set; } = string.Empty;
        public int Number { get; set; }
    }
}
