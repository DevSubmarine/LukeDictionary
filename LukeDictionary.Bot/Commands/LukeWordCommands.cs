﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevSubmarine.LukeDictionary.Commands
{
    public class LukeWordCommands
    {
        private readonly DiscordClient _client;
        private readonly ILukeWordsStore _store;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<DevSubmarineOptions> _devsubOptions;

        public LukeWordCommands(DiscordClient client, ILukeWordsStore store, ILogger<LukeWordCommands> log,
            IOptionsMonitor<DevSubmarineOptions> devsubOptions)
        {
            this._client = client;
            this._store = store;
            this._log = log;
            this._devsubOptions = devsubOptions;
        }

        public async Task<DiscordEmbed> BuildWordEmbedAsync(LukeWord word, DiscordGuild guild, CancellationToken cancellationToken = default)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            DiscordUser luke = await this._client.GetUserAsync(this._devsubOptions.CurrentValue.LukeUserID).ConfigureAwait(false);
            string lukeName = await this.GetUserNameAsync(luke.Id, guild, cancellationToken).ConfigureAwait(false);
            string addedByName = await this.GetUserNameAsync(word.AddedByUserID, guild, cancellationToken).ConfigureAwait(false);

            return new DiscordEmbedBuilder()
                .AddField("The Word!", $"***{word}***", false)
                .AddField("Word By", $"{lukeName} (obviously <:what:526104145728503808>)", true)
                .AddField("Added By", addedByName, true)
                .WithThumbnail(luke.AvatarUrl)
                .WithTimestamp(word.CreationTimeUTC)
                .WithFooter($"This amazing word was invented by {luke.Username}#{luke.Discriminator}", luke.AvatarUrl)
                .Build();
        }

        public async Task<string> GetUserNameAsync(ulong id, DiscordGuild guild, CancellationToken cancellationToken = default)
        {
            DiscordUser user = null;
            if (guild != null)
                user = await guild.GetMemberSafeAsync(id).ConfigureAwait(false);
            if (user != null)
                return Formatter.Mention(user, true);

            // if not a member, get as user and build string
            user = await this._client.GetUserAsync(id).ConfigureAwait(false);
            return $"{user.Username}#{user.Discriminator}";
        }

        public async Task<LukeWord> AddOrGetWordAsync(LukeWord word, CancellationToken cancellationToken = default)
        {
            // try get existing one first
            LukeWord result = await this._store.GetWordAsync(word.ToString(), cancellationToken).ConfigureAwait(false);
            if (result != null)
                return result;

            await this._store.AddWordAsync(word, cancellationToken).ConfigureAwait(false);
            return word;
        }


        // now, DSharpPlus decided to use base classes instead of interfaces for everything :face_vomiting: 
        // for this reason, we need to create separate "module" classes for simple and slash commands
        // sigh ... bad design, bad design everywhere
        [SlashCommandGroup("lukedict", "Commands for accessing Luke's wonderful dictionary")]
        private class LukeWordsCommandsSlashModule : SlashCommandModule
        {
            private readonly LukeWordCommands _shared;

            public LukeWordsCommandsSlashModule(IServiceProvider services)
            {
                this._shared = ActivatorUtilities.CreateInstance<LukeWordCommands>(services);
            }

            [SlashCommand("add", "Adds a new word to Luke Dictionary")]
            public async Task CmdAdd(InteractionContext context, [Option("word", "Word to add")] string word)
            {
                word = word.Trim().ToLowerInvariant();
                DiscordUser user = context.Member ?? context.User;

                LukeWord result = new LukeWord(word, user.Id);
                result.GuildID = context.Guild?.Id;
                result.ChannelID = context.Channel?.Id;
                result.MessageID = context.InteractionId;

                result = await this._shared.AddOrGetWordAsync(result).ConfigureAwait(false);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .AddEmbed(await this._shared.BuildWordEmbedAsync(result, context.Guild)))
                    .ConfigureAwait(false);
            }
        }

        [ModuleLifespan(ModuleLifespan.Transient)]
        private class LukeWordsCommandsStandardModule : BaseCommandModule
        {
            private readonly LukeWordCommands _shared;

            public LukeWordsCommandsStandardModule(IServiceProvider services)
            {
                this._shared = ActivatorUtilities.CreateInstance<LukeWordCommands>(services);
            }

            [Command("lukedict")]
            [Priority(-10)] // lower priority to allow additional commands like "likedict count" or whatever
                            // TODO: DSharpPlus commands handling is garbage (unlike Wolfringo's), and doesn't allow whitespaces - add a "mode" switch to this command to support "add" etc
            public async Task CmdAdd(CommandContext context, string word)
            {
                word = word.Trim().ToLowerInvariant();
                DiscordUser user = context.Member ?? context.User;

                LukeWord result = new LukeWord(word, user.Id);
                result.GuildID = context.Guild?.Id;
                result.ChannelID = context.Channel?.Id;
                result.MessageID = context.Message?.Id;

                result = await this._shared.AddOrGetWordAsync(result).ConfigureAwait(false);
                await context.RespondAsync(await this._shared.BuildWordEmbedAsync(result, context.Guild)).ConfigureAwait(false);
            }
        }
    }
}
