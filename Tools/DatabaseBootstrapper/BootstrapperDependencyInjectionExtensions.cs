using DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper.CollectionCreators;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BootstrapperDependencyInjectionExtensions
    {
        public static IServiceCollection AddCollectionCreator<T>(this IServiceCollection services) where T : class, ICollectionCreator
            => services.AddTransient<ICollectionCreator, T>();
    }
}
