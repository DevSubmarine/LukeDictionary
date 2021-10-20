using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;

namespace DevSubmarine.LukeDictionary.Database
{
    public static class MongoExtensions
    {
        public static IMongoDatabase GetDatabase(this IMongoDatabaseClient client, string name, MongoDatabaseSettings settings = null)
            => client.Client.GetDatabase(name, settings);

        // client is noop - merely for ease of access
        public static void RegisterClassMap<T>(this IMongoDatabaseClient client, Action<BsonClassMap<T>> classMapInitializer)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                if (classMapInitializer == null)
                    BsonClassMap.RegisterClassMap<T>();
                else
                    BsonClassMap.RegisterClassMap<T>(classMapInitializer);
            }
        }

        // client is noop - merely for ease of access
        public static void RegisterClassMap<T>(this IMongoDatabaseClient client)
            => RegisterClassMap<T>(null);
    }
}
