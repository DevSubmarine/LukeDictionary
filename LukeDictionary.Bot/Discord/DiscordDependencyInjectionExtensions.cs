using System;
using DevSubmarine.LukeDictionary.Discord;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>Discord related extensions for dependency injection container.</summary>
    public static class DiscordDependencyInjectionExtensions
    {
        /// <summary>Adds Discord related services.</summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Optional delegate for inline options configuration.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddDiscord(this IServiceCollection services, Action<DiscordOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.TryAddSingleton<IValidateOptions<DiscordOptions>, DiscordOptionsValidator>();

            services.TryAddSingleton<IHostedDiscordClient, HostedDiscordClient>();
            services.AddTransient<IHostedService>(provider => provider.GetRequiredService<IHostedDiscordClient>());
            services.TryAddSingleton<DiscordClient>(provider => provider.GetRequiredService<IHostedDiscordClient>().Client);

            return services;
        }
    }
}
