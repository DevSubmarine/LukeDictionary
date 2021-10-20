using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DevSubmarine.LukeDictionary
{
    public static class DiscordLoggingExtensions
    {
        public static IDisposable BeginCommandScope(this ILogger log, CommandContext context, object handler = null, [CallerMemberName] string cmdName = null)
            => BeginCommandScope(log, context, handler?.GetType(), cmdName);
        public static IDisposable BeginCommandScope(this ILogger log, CommandContext context, Type handlerType = null, [CallerMemberName] string cmdName = null)
        {
            Dictionary<string, object> state = new Dictionary<string, object>
            {
                { "Command.UserID", context.User?.Id },
                { "Command.MessageID", context.Message?.Id },
                { "Command.ChannelID", context.Channel?.Id },
                { "Command.GuildID", context.Guild?.Id }
            };
            if (!string.IsNullOrWhiteSpace(context.Command.Name))
                state.Add("Command.DisplayName", context.Command.Name);
            if (!string.IsNullOrWhiteSpace(cmdName))
                state.Add("Command.Method", cmdName);
            if (handlerType != null)
                state.Add("Command.Handler", handlerType.Name);
            return log.BeginScope(state);
        }

        public static IDisposable BeginCommandScope(this ILogger log, InteractionContext context, object handler = null, [CallerMemberName] string cmdName = null)
            => BeginCommandScope(log, context, handler?.GetType(), cmdName);
        public static IDisposable BeginCommandScope(this ILogger log, InteractionContext context, Type handlerType = null, [CallerMemberName] string cmdName = null)
        {
            Dictionary<string, object> state = new Dictionary<string, object>
            {
                { "Command.UserID", context.User?.Id },
                { "Command.InteractionID", context.Interaction?.Id },
                { "Command.ApplicationID", context.Interaction?.ApplicationId },
                { "Command.ChannelID", context.Channel?.Id },
                { "Command.GuildID", context.Guild?.Id }
            };
            if (!string.IsNullOrWhiteSpace(context.CommandName))
                state.Add("Command.DisplayName", context.CommandName);
            if (!string.IsNullOrWhiteSpace(cmdName))
                state.Add("Command.Method", cmdName);
            if (handlerType != null)
                state.Add("Command.Handler", handlerType.Name);
            return log.BeginScope(state);
        }

        public static IDisposable BeginCommandScope(this ILogger log, MessageCreateEventArgs context, object handler = null, [CallerMemberName] string cmdName = null)
            => BeginCommandScope(log, context, handler?.GetType(), cmdName);

        public static IDisposable BeginCommandScope(this ILogger log, MessageCreateEventArgs context, Type handlerType = null, [CallerMemberName] string cmdName = null)
        {
            Dictionary<string, object> state = new Dictionary<string, object>
            {
                { "Command.UserID", context.Author?.Id },
                { "Command.MessageID", context.Message?.Id },
                { "Command.ChannelID", context.Channel?.Id },
                { "Command.GuildID", context.Guild?.Id }
            };
            if (!string.IsNullOrWhiteSpace(cmdName))
                state.Add("Command.Method", cmdName);
            if (handlerType != null)
                state.Add("Command.Handler", handlerType.Name);
            return log.BeginScope(state);
        }
    }
}
