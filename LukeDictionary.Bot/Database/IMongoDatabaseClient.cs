using MongoDB.Driver;

namespace DevSubmarine.LukeDictionary.Database
{
    /// <summary>A hosted wrapper for MongoDB client.</summary>
    public interface IMongoDatabaseClient
    {
        /// <summary>Underlying client.</summary>
        MongoClient Client { get; }

        /// <summary>Gets collection from default database specified by options.</summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="name">The name of the collection.</param>
        /// <param name="settings">The settings.</param>
        /// <returns> An implementation of a collection.</returns>
        // I took these above from official MongoDB comments, don't shout at me lol.
        IMongoCollection<TDocument> GetCollection<TDocument>(string name, MongoCollectionSettings settings = null);
    }
}
