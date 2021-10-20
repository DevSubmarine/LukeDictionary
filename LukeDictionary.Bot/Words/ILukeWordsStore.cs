using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevSubmarine.LukeDictionary
{
    public interface ILukeWordsStore
    {
        Task AddWordAsync(LukeWord word, CancellationToken cancellationToken = default);
        Task<LukeWord> GetWordAsync(string word, CancellationToken cancellationToken = default);
        Task<IEnumerable<LukeWord>> GetAllWordsAsync(CancellationToken cancellationToken = default);
        Task<LukeWord> GetRandomWordAsync(CancellationToken cancellationToken = default);
    }
}
