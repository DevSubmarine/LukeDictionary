using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevSubmarine.LukeDictionary.Discord
{
    /// <summary>A hosted wrapper for Discord client.</summary>
    public class HostedDiscordClient : IHostedDiscordClient, IHostedService, IDisposable
    {
        /// <summary>Underlying Discord client.</summary>
        public DiscordClient Client { get; private set; }

        private readonly ILogger _log;
        private readonly ILoggerFactory _logFactory;
        private readonly IOptionsMonitor<DiscordOptions> _discordOptions;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private bool _started = false;

        public HostedDiscordClient(IOptionsMonitor<DiscordOptions> discordOptions, ILogger<HostedDiscordClient> log, ILoggerFactory logFactory)
        {
            this._discordOptions = discordOptions;
            this._log = log;
            this._logFactory = logFactory;

            this._log.LogDebug("Creating Discord client");
            DiscordConfiguration clientConfig = new DiscordConfiguration();
            clientConfig.AutoReconnect = true;
            clientConfig.Intents = DiscordIntents.All | DiscordIntents.AllUnprivileged;
            clientConfig.LoggerFactory = this._logFactory;
            clientConfig.MessageCacheSize = 512;
            clientConfig.Token = this._discordOptions.CurrentValue.BotToken;
            clientConfig.TokenType = TokenType.Bot;
            this.Client = new DiscordClient(clientConfig);
        }

        /// <inheritdoc/>
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_started)
                    return;

                _started = true;
                await this.Client.ConnectAsync().ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }

        /// <inheritdoc/>
        async Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this.Client == null)
                    return;

                await this.Client.DisconnectAsync().ConfigureAwait(false);
                this.Dispose();
            }
            finally
            {
                this._lock.Release();
            }
        }

        /// <summary>Retrieves underlying Discord client.</summary>
        /// <param name="client">Hosted client wrapper.</param>
        public static implicit operator DiscordClient(HostedDiscordClient client)
            => client.Client;

        /// <inheritdoc/>
        public void Dispose()
        {
            try { this.Client?.Dispose(); } catch { }
            this.Client = null;
        }
    }
}
