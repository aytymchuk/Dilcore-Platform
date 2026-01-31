namespace Dilcore.Tenancy.Store;

/// <summary>
/// Configuration settings for MongoDB.
/// </summary>
public sealed class MongoDbSettings
{
    /// <summary>
    /// MongoDB connection string.
    /// </summary>
    public required string ConnectionString { get; init; }
}
