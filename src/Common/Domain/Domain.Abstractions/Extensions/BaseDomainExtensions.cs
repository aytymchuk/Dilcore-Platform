namespace Dilcore.Domain.Abstractions.Extensions;

public static class BaseDomainExtensions
{
    public static T UpdateETag<T>(this T domain, TimeProvider timeProvider) where T : BaseDomain
    {
        return domain with { ETag = timeProvider.GetUtcNow().ToUnixTimeMilliseconds() };
    }

    public static T SetCreatedAt<T>(this T domain, TimeProvider timeProvider) where T : BaseDomain
    {
        return domain with { CreatedAt = timeProvider.GetUtcNow().UtcDateTime };
    }

    public static T SetUpdatedAt<T>(this T domain, TimeProvider timeProvider) where T : BaseDomain
    {
        return domain with { UpdatedAt = timeProvider.GetUtcNow().UtcDateTime };
    }
}