using DSharpPlus;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.LukeDictionary.Discord
{
    /// <summary>A hosted wrapper for Discord client.</summary>
    public interface IHostedDiscordClient : IHostedService
    {
        /// <summary>Underlying Discord client.</summary>
        DiscordClient Client { get; }
    }
}
