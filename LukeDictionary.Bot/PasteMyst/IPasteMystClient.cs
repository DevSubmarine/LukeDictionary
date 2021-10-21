using System.Threading;
using System.Threading.Tasks;

namespace DevSubmarine.LukeDictionary.PasteMyst
{
    public interface IPasteMystClient
    {
        Task<Paste> CreatePasteAsync(Paste paste, CancellationToken cancellationToken = default);
    }
}
