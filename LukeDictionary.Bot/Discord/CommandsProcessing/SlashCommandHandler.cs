using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevSubmarine.LukeDictionary.Discord.CommandsProcessing
{
    public class SlashCommandHandler : IHostedService, IDisposable
    {
        private DiscordClient _client;
        private IOptionsMonitor<DiscordOptions> _options;
        private IServiceProvider _serviceProvider;
        private ILogger _log;
        private SemaphoreSlim _lock;
        private CancellationToken _hostCancellationToken;

        public SlashCommandHandler(IServiceProvider serviceProvider, IHostedDiscordClient client, IOptionsMonitor<DiscordOptions> options, ILogger<SlashCommandHandler> log)
        {
            this._client = client.Client;
            this._options = options;
            this._serviceProvider = serviceProvider;
            this._log = log;
            this._lock = new SemaphoreSlim(1, 1);

            _options.OnChange(async _ => await InitializeCommandsAsync());
        }

        private Task InitializeCommandsAsync()
        {
            DiscordOptions options = this._options.CurrentValue;
            if (!options.EnableSlashCommands)
            {
                this._log.LogDebug("Slash commands disabled in options");
                return Task.CompletedTask;
            }

            this._log.LogDebug("Initializing Slash commands");
            SlashCommandsExtension slashCommands = this._client.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = this._serviceProvider
            });

            IEnumerable<TypeInfo> commands = this.FindCommands(Assembly.GetEntryAssembly());
            foreach (TypeInfo type in commands)
                slashCommands.RegisterCommands(type, options.GuildID);

            return Task.CompletedTask;
        }

        private IEnumerable<TypeInfo> FindCommands(Assembly assembly)
        {
            IEnumerable<TypeInfo> types = assembly.DefinedTypes.Where(t => !t.IsAbstract && !t.ContainsGenericParameters
                && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) && typeof(SlashCommandModule).IsAssignableFrom(t));
            if (!types.Any())
                this._log.LogWarning("Cannot initialize Slash commands from assembly {AssemblyName} - no non-static non-abstract classes inheriting from {Class}", assembly.FullName, nameof(SlashCommandModule));
            return types;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
            => InitializeCommandsAsync();

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // the only way to cancel slash commands as of 1.7.6 is resetting them
            try { this._client.UseSlashCommands(); } catch { }
        }
    }
}
