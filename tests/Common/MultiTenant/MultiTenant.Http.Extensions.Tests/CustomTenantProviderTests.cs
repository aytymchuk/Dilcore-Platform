using Dilcore.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Dilcore.MultiTenant.Http.Extensions.Tests;

/// <summary>
/// Example custom provider for testing/demonstration
/// </summary>
public class StaticTenantProvider : ITenantContextProvider
{
    private readonly ITenantContext? _context;

    public int Priority { get; }

    public StaticTenantProvider(ITenantContext? context, int priority = 100)
    {
        _context = context;
        Priority = priority;
    }

    public ITenantContext? GetTenantContext() => _context;
}

[TestFixture]
public class CustomTenantProviderTests
{
    [Test]
    public void CustomProvider_ReturnsContext()
    {
        // Arrange
        var customContext = new TenantContext("Custom", "custom-storage");
        var provider = new StaticTenantProvider(customContext, priority: 150);

        var resolver = new TenantContextResolver(new[] { provider }, NullLogger<TenantContextResolver>.Instance);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.ShouldBe(customContext);
        result.Name.ShouldBe("Custom");
        result.StorageIdentifier.ShouldBe("custom-storage");
    }

    [Test]
    public void CustomProvider_HigherPriority_BeatsHttp()
    {
        // Arrange
        var httpContext = new TenantContext("Http", "http-storage");
        var httpProvider = new StaticTenantProvider(httpContext, priority: 100);

        var customContext = new TenantContext("Custom", "custom-storage");
        var customProvider = new StaticTenantProvider(customContext, priority: 200);

        var resolver = new TenantContextResolver(
            new ITenantContextProvider[] { httpProvider, customProvider },
            NullLogger<TenantContextResolver>.Instance);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.ShouldBe(customContext); // Custom wins
        result.Name.ShouldBe("Custom");
    }

    [Test]
    public void CustomProvider_NullContext_FallsBackToNext()
    {
        // Arrange
        var customProvider = new StaticTenantProvider(null, priority: 200); // Returns null

        var httpContext = new TenantContext("Http", "http-storage");
        var httpProvider = new StaticTenantProvider(httpContext, priority: 100);

        var resolver = new TenantContextResolver(
            new ITenantContextProvider[] { customProvider, httpProvider },
            NullLogger<TenantContextResolver>.Instance);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.ShouldBe(httpContext); // Fallback to HTTP
        result.Name.ShouldBe("Http");
    }
}