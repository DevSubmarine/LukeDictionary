using System;
using DevSubmarine.LukeDictionary.PasteMyst;
using DevSubmarine.LukeDictionary.PasteMyst.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PasteMystDependencyInjectionExtensions
    {
        public static IServiceCollection AddPasteMyst(this IServiceCollection services, Action<PasteMystOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddHttpClient<IPasteMystClient, PasteMystClient>();

            return services;
        }
    }
}
