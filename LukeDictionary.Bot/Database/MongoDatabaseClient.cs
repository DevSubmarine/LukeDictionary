using System;
using DevSubmarine.LukeDictionary.Database.Conventions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace DevSubmarine.LukeDictionary.Database.Services
{
    public class MongoDatabaseClient : IMongoDatabaseClient
    {
        private Lazy<MongoClient> _client;
        public MongoClient Client => this._client.Value;

        private readonly MongoOptions _options;
        private readonly ILogger<MongoDatabaseClient> _log;

        public MongoDatabaseClient(IOptions<MongoOptions> options, ILogger<MongoDatabaseClient> log)
        {
            this._options = options.Value;
            this._log = log;

            this._client = new Lazy<MongoClient>(() =>
            {
                this._log.LogTrace("Establishing connection to MongoDB...");
                FixMongoMapping();
                return new MongoClient(this._options.ConnectionString);
            });
        }

        private static void FixMongoMapping()
        {
            // because ImmutableTypeClassMapConvention messes up when there's an object that has only readonly props
            // we need to remove it. To do it, we need to... unregister default conventions and re-register them manually... sigh
            // ref: https://www.codewrecks.com/post/nosql/replace-immutable-serializer-in-mongodb/
            // src of default conventions: https://github.com/mongodb/mongo-csharp-driver/blob/master/src/MongoDB.Bson/Serialization/Conventions/DefaultConventionPack.cs
            const string packName = "__defaults__";
            ConventionRegistry.Remove(packName);

            // init mongodb mapping conventions
            ConventionPack conventions = new ConventionPack();
            conventions.Add(new ReadWriteMemberFinderConvention());
            conventions.Add(new NamedIdMemberConvention(new[] { "Id", "id", "_id", "ID" }));    // adding "ID" as a bonus here
            conventions.Add(new NamedExtraElementsMemberConvention(new[] { "ExtraElements" }));
            conventions.Add(new IgnoreExtraElementsConvention(true));   // bonus - don't throw if not all properties match
            conventions.Add(new NamedParameterCreatorMapConvention());
            conventions.Add(new StringObjectIdIdGeneratorConvention());
            conventions.Add(new LookupIdGeneratorConvention());
            // custom conventions
            conventions.Add(new MapReadOnlyPropertiesConvention());
            conventions.Add(new GuidAsStandardRepresentationConvention());
            ConventionRegistry.Register(packName, conventions, _ => true);

            // guid serialization
            #pragma warning disable CS0618 // Type or member is obsolete
            BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
            #pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
