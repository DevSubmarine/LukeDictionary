using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Database;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public MongoLukeWordsStore(IMongoDatabaseClient client, IMemoryCache cache, IOptions<MongoOptions> databaseOptions, ILogger<MongoLukeWordsStore> log)
        {
            this._log = log;
            this._collection = client.GetCollection<LukeWord>(databaseOptions.Value.WordsCollectionName);
            this._cache = cache;
            this._cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(6));
        }

        public async Task AddWordAsync(LukeWord word, CancellationToken cancellationToken = default)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            this._log.LogDebug("Inserting Luke's amazing word {Word} to DB", word);
            await this._collection.InsertOneAsync(word, null, cancellationToken).ConfigureAwait(false);
            this._cache.Set(word.ToString(), word, this._cacheOptions);
        }

        public async Task<LukeWord> GetWordAsync(string word, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));

            string wordLowercase = word.Trim().ToLowerInvariant();
            if (!this._cache.TryGetValue(wordLowercase, out LukeWord result))
            {
                this._log.LogDebug("Retrieving Luke's amazing word {word} from DB", word);
                result = await this._collection.Find(db => db.Word == wordLowercase).FirstOrDefaultAsync(cancellationToken);
                this._cache.Set(wordLowercase, result, this._cacheOptions);
            }
            return result;
        }

        public async Task<IEnumerable<LukeWord>> GetAllWordsAsync(CancellationToken cancellationToken = default)
        {
            this._log.LogDebug("Retrieving all Luke's amazing words from DB");
            IAsyncCursor<LukeWord> words = await this._collection.Find(Builders<LukeWord>.Filter.Empty).ToCursorAsync(cancellationToken).ConfigureAwait(false);
            return await words.ToListAsync(cancellationToken).ConfigureAwait(false);    // ToList cause ToEnumerable complains when iterated more than once
        }

        public Task<LukeWord> GetRandomWordAsync(CancellationToken cancellationToken = default)
            => this._collection.AsQueryable().Sample(1).FirstOrDefaultAsync(cancellationToken);

        public Task<long> GetWordsCountAsync(CancellationToken cancellationToken = default)
            => this._collection.CountDocumentsAsync(Builders<LukeWord>.Filter.Empty, null, cancellationToken);
    }
}
