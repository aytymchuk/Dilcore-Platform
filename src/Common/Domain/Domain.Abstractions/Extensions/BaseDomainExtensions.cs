namespace Dilcore.Common.Domain.Abstractions.Extensions;

public static class BaseDomainExtensions
{
    public static T UpdateETag<T>(this T domain, TimeProvider timeProvider) where T : BaseDomain
    {
        return domain with { ETag = timeProvider.GetUtcNow().ToUnixTimeMilliseconds() };
    }

    public static T SetCreatedOn<T>(this T domain, TimeProvider timeProvider) where T : BaseDomain
    {
        return domain with { CreatedOn = timeProvider.GetUtcNow().UtcDateTime };
    }

    public static T SetUpdatedOn<T>(this T domain, TimeProvider timeProvider) where T : BaseDomain
    {
        return domain with { UpdatedOn = timeProvider.GetUtcNow().UtcDateTime };
    }
}