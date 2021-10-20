using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevSubmarine.LukeDictionary.Discord.CommandsProcessing
{
    public class SimpleCommandHandler : IHostedService, IDisposable
    {
        public IEnumerable<Command> Commands => this._client.GetCommandsNext().RegisteredCommands.Values;

        private DiscordClient _client;
        private IOptionsMonitor<DiscordOptions> _options;
        private IServiceProvider _serviceProvider;
        private ILogger _log;

        public SimpleCommandHandler(IServiceProvider serviceProvider, IHostedDiscordClient client, IOptionsMonitor<DiscordOptions> options, ILogger<SimpleCommandHandler> log)
        {
            this._client = client.Client;
            this._options = options;
            this._serviceProvider = serviceProvider;
            this._log = log;

            _options.OnChange(async _ => await InitializeCommandsAsync());

            this._client.MessageCreated += HandleCommandAsync;
        }

        private Task InitializeCommandsAsync()
        {
            DiscordOptions options = this._options.CurrentValue;
            if (!options.EnableSimpleCommands)
            {
                this._log.LogDebug("Simple commands disabled in options");
                return Task.CompletedTask;
            }

            this._log.LogDebug("Initializing Simple commands");
            CommandsNextExtension commandsNext = this._client.UseCommandsNext(new CommandsNextConfiguration()
            {
                UseDefaultCommandHandler = false,
                Services = this._serviceProvider,
                EnableDms = true,
                CaseSensitive = false,
                EnableDefaultHelp = false,
                DmHelp = false,
                IgnoreExtraArguments = true,
                EnableMentionPrefix = options.AcceptMentionPrefix,
                StringPrefixes = new string[] { options.Prefix }
            });

            IEnumerable<TypeInfo> commands = this.FindCommands(Assembly.GetEntryAssembly());
            foreach (TypeInfo type in commands)
                commandsNext.RegisterCommands(type);

            return Task.CompletedTask;
        }

        // loading commands in DSharpPlus is shit and doesn't support nested classes, so let's do it properly by ourselves
        // sigh, can we get at least one decent C# library for Discord? I mean, besides https://github.com/TehGM/Discord.Interactions.AspNetCore ofc
        private IEnumerable<TypeInfo> FindCommands(Assembly assembly)
        {
            IEnumerable<TypeInfo> types = assembly.DefinedTypes.Where(t => !t.IsAbstract && !t.ContainsGenericParameters
                && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) && typeof(BaseCommandModule).IsAssignableFrom(t));
            if (!types.Any())
                this._log.LogWarning("Cannot initialize Simple commands from assembly {AssemblyName} - no non-static non-abstract classes inheriting from {Class}", assembly.FullName, nameof(BaseCommandModule));
            return types;
        }

        private Task HandleCommandAsync(DiscordClient sender, MessageCreateEventArgs e)
        {
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            DiscordOptions options = this._options.CurrentValue;
            // only execute if not a bot message
            if (options.IgnoreBotMessages && e.Message.Author.IsBot)
                return Task.CompletedTask;

            // Don't process the command if it was a system message
            if (e.Message.MessageType != MessageType.Default && e.Message.MessageType == MessageType.Reply)
                return Task.CompletedTask;

            // get prefix and argPos
            bool requirePrefix = e.Guild != null ? options.RequirePublicMessagePrefix : options.RequirePrivateMessagePrefix;
            int argPos = e.Message.GetStringPrefixLength(options.Prefix, StringComparison.OrdinalIgnoreCase);
            if (argPos == -1 && options.AcceptMentionPrefix)
                argPos = e.Message.GetMentionPrefixLength(sender.CurrentUser);

            // if prefix not found but is required, return
            if (requirePrefix && argPos == -1)
                return Task.CompletedTask;

            // separate prefix from content
            string prefix = string.Empty;
            string content = e.Message.Content;
            if (argPos > 0)
            {
                prefix = e.Message.Content.Remove(0, argPos);
                content = e.Message.Content.Substring(argPos);
            }

            // find command
            CommandsNextExtension commandsNext = this._client.GetCommandsNext();
            Command command = commandsNext.FindCommand(content, out string args);
            if (command == null)
                return Task.CompletedTask;

            // create context and execute the command
            CommandContext ctx = commandsNext.CreateContext(e.Message, prefix, command, args);
            _ = Task.Run(() => commandsNext.ExecuteCommandAsync(ctx));
            return Task.CompletedTask;
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
            try { this._client.MessageCreated -= HandleCommandAsync; } catch { }
        }
    }
}
