using MongoDB.Driver;

namespace DevSubmarine.LukeDictionary.Database
{
    /// <summary>A hosted wrapper for MongoDB client.</summary>
    public interface IMongoDatabaseClient
    {
        /// <summary>Underlying client.</summary>
        MongoClient Client { get; }
    }
}
