using System.Collections.Generic;
using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Database;
using DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper.CollectionCreators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper
{
    public class ApplicationRunner
    {
        private readonly MongoClient _client;
        private readonly MongoOptions _options;
        private readonly ILogger _log;
        private readonly IEnumerable<ICollectionCreator> _collectionCreators;

        public ApplicationRunner(IOptions<MongoOptions> options, ILogger<ApplicationRunner> log, IMongoDatabaseClient client,
            IEnumerable<ICollectionCreator> collectionCreators)
        {
            this._options = options.Value;
            this._log = log;
            this._client = client.Client;
            this._collectionCreators = collectionCreators;
        }

        public async Task RunAsync()
        {
            this._log.LogInformation("Connecting to the database {Database}", this._options.DatabaseName);
            IMongoDatabase db = this._client.GetDatabase(this._options.DatabaseName);

            foreach (ICollectionCreator collectionCreator in this._collectionCreators)
                await collectionCreator.ProcessCollectionAsync().ConfigureAwait(false);

            this._log.LogInformation("Done");
        }
    }
}
