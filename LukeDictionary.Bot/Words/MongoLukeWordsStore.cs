using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DevSubmarine.LukeDictionary.Services
{
    public class MongoLukeWordsStore : ILukeWordsStore
    {
        private readonly ILogger _log;
        private readonly IMongoCollection<LukeWord> _collection;

        public MongoLukeWordsStore(IMongoDatabaseClient client, IOptions<MongoOptions> databaseOptions, ILogger<MongoLukeWordsStore> log)
        {
            this._log = log;
            this._collection = client.GetCollection<LukeWord>(databaseOptions.Value.WordsCollectionName);
        }

        public Task AddWordAsync(LukeWord word, CancellationToken cancellationToken = default)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            this._log.LogTrace("Inserting Luke's amazing word {Word} to DB", word);
            return this._collection.InsertOneAsync(word, null, cancellationToken);
        }

        public Task<LukeWord> GetWordAsync(string word, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));

            string wordLowercase = word.Trim().ToLowerInvariant();

            return this._collection.Find(db => db.Word == wordLowercase).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<LukeWord>> GetAllWordsAsync(CancellationToken cancellationToken = default)
        {
            IAsyncCursor<LukeWord> words = await this._collection.Find(Builders<LukeWord>.Filter.Empty).ToCursorAsync(cancellationToken).ConfigureAwait(false);
            return words.ToEnumerable(cancellationToken);
        }

        public Task<LukeWord> GetRandomWordAsync(CancellationToken cancellationToken = default)
            => this._collection.AsQueryable().Sample(1).FirstOrDefaultAsync(cancellationToken);
    }
}
