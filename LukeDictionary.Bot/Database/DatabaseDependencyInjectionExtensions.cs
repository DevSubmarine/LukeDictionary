using System;
using DevSubmarine.LukeDictionary;
using DevSubmarine.LukeDictionary.Database;
using DevSubmarine.LukeDictionary.Database.Services;
using DevSubmarine.LukeDictionary.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseDependencyInjectionExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMongoDatabaseClient, MongoDatabaseClient>();
            services.TryAddTransient<ILukeWordsStore, MongoLukeWordsStore>();

            return services;
        }
    }
}
