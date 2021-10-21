using System.Threading;
using System.Threading.Tasks;

namespace DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper.CollectionCreators
{
    public interface ICollectionCreator
    {
        Task ProcessCollectionAsync(CancellationToken cancellationToken = default);
    }
}
