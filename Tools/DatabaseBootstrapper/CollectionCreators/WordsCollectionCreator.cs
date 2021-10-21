using System.Threading;
using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper.CollectionCreators
{
    class WordsCollectionCreator : CollectionCreatorBase<LukeWord>
    {
        protected override string CollectionName { get; }

        public WordsCollectionCreator(IMongoDatabaseClient client, ILogger<WordsCollectionCreator> log, IOptions<MongoOptions> options)
            : base(client, log, options)
        {
            this.CollectionName = options.Value.WordsCollectionName;
        }

        public override async Task ProcessCollectionAsync(CancellationToken cancellationToken = default)
        {
            IMongoCollection<LukeWord> collection = await base.GetOrCreateCollectionAsync(cancellationToken).ConfigureAwait(false);

            // do other stuff like creating indexes etc
            // add as needed, if needed
        }
    }
}
