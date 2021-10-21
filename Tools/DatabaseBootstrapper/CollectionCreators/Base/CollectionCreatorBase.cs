using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using DevSubmarine.LukeDictionary.Database;

namespace DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper.CollectionCreators
{
    public abstract class CollectionCreatorBase<T> : ICollectionCreator
    {
        protected abstract string CollectionName { get; }
        protected IMongoDatabase Database { get; }
        protected ILogger Log { get; }

        public CollectionCreatorBase(IMongoDatabase database, ILogger log)
        {
            this.Database = database;
            this.Log = log;
        }

        public CollectionCreatorBase(IMongoDatabaseClient client, ILogger log, IOptions<MongoOptions> options)
            : this(client.Client.GetDatabase(options.Value.DatabaseName), log) { }

        public abstract Task ProcessCollectionAsync(CancellationToken cancellationToken = default);

        protected async Task<IMongoCollection<T>> GetOrCreateCollectionAsync(CancellationToken cancellationToken = default)
        {
            this.Log.LogInformation("Getting collection {Collection}", CollectionName);
            if (!await this.CollectionExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                this.Log.LogDebug("Collection {Collection} does not exist, creating", CollectionName);
                await this.Database.CreateCollectionAsync(CollectionName, null, cancellationToken).ConfigureAwait(false);
            }
            return this.Database.GetCollection<T>(CollectionName);
        }

        protected async Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
        {
            IAsyncCursor<string> collections = await this.Database.ListCollectionNamesAsync(
                new ListCollectionNamesOptions { Filter = new BsonDocument("name", CollectionName) },
                cancellationToken).ConfigureAwait(false);
            return await collections.AnyAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
