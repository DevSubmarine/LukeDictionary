using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Discord;
using DevSubmarine.LukeDictionary.PasteMyst;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DevSubmarine.LukeDictionary.Commands
{
    public class LukeWordCommands
    {
        private readonly DiscordClient _client;
        private readonly ILukeWordsStore _store;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<DevSubmarineOptions> _devsubOptions;
        private readonly IPasteMystClient _pasteMyst;
        private readonly Regex _inputValidationRegex;

        public LukeWordCommands(DiscordClient client, ILukeWordsStore store, ILogger<LukeWordCommands> log,
            IOptionsMonitor<DevSubmarineOptions> devsubOptions, IPasteMystClient pasteMyst)
        {
            this._client = client;
            this._store = store;
            this._log = log;
            this._devsubOptions = devsubOptions;
            this._pasteMyst = pasteMyst;
            this._inputValidationRegex = new Regex(@"^[A-Za-z0-9]{1,25}$", 
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, 
                TimeSpan.FromMilliseconds(500));
        }

        public async Task<DiscordEmbed> BuildWordEmbedAsync(LukeWord word, DiscordGuild guild, CancellationToken cancellationToken = default)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            DiscordUser luke = await this._client.GetUserAsync(this._devsubOptions.CurrentValue.LukeUserID).ConfigureAwait(false);
            string lukeName = await this.GetUserNameAsync(luke.Id, guild, cancellationToken).ConfigureAwait(false);
            string addedByName = await this.GetUserNameAsync(word.AddedByUserID, guild, cancellationToken).ConfigureAwait(false);

            return new DiscordEmbedBuilder()
                .WithTitle(word.ToString())
                .AddField("Word By", $"{lukeName} (obviously {ResponseEmoji.JerryWhat})", true)
                .AddField("Added By", addedByName, true)
                .WithThumbnail(luke.AvatarUrl)
                .WithTimestamp(word.CreationTimeUTC)
                .WithFooter($"This amazing word was invented by {luke.Username}#{luke.Discriminator}", luke.AvatarUrl)
                .WithUrl($"https://discord.com/channels/{word.GuildID}/{word.ChannelID}/{word.MessageID}")
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
            LukeWord result = await this.GetWordAsync(word.ToString(), cancellationToken).ConfigureAwait(false);
            if (result != null)
                return result;

            await this._store.AddWordAsync(word, cancellationToken).ConfigureAwait(false);
            return word;
        }

        public Task<LukeWord> GetWordAsync(string word, CancellationToken cancellationToken = default)
            => this._store.GetWordAsync(word, cancellationToken);

        public Task<LukeWord> GetRandomWordAsync(CancellationToken cancellationToken = default)
            => this._store.GetRandomWordAsync(cancellationToken);

        public Task<long> GetWordsCountAsync(CancellationToken cancellationToken = default)
            => this._store.GetWordsCountAsync(cancellationToken);

        public async Task<string> GetWordsListAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<LukeWord> words = await this._store.GetAllWordsAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                string title = $"Luke's Dictionary vol. {DateTime.UtcNow}";
                string contentPlain = $"Words in Luke Dictionary: {words.Count()}\r\n\r\n{string.Join("\r\n", words)}";
                string contentJson = new JArray(words.Select(w => JObject.FromObject(w))).ToString(Newtonsoft.Json.Formatting.Indented);

                Paste paste = new Paste(title, new Pasty[]
                {
                    new Pasty(contentPlain, $"{title} (Plain Text)", PastyLanguages.PlainText),
                    new Pasty(contentJson, $"{title} (JSON)", PastyLanguages.JSON)
                });
                paste = await this._pasteMyst.CreatePasteAsync(paste, cancellationToken).ConfigureAwait(false);
                return $"https://paste.myst.rs/{paste.ID}";
            }
            catch (HttpRequestException) { throw; }
            catch (Exception ex) when (ex.LogAsError(this._log, "Failed building words list")) { throw; }
        }

        public bool ValidateInput(LukeWord word)
            => this.ValidateInput(word?.ToString());

        public bool ValidateInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            return this._inputValidationRegex.IsMatch(input);
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
                DiscordUser user = context.Member ?? context.User;

                LukeWord result = new LukeWord(word, user.Id);
                result.GuildID = context.Guild?.Id;
                result.ChannelID = context.Channel?.Id;
                result.MessageID = context.InteractionId;

                if (!this._shared.ValidateInput(result))
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent($"{ResponseEmoji.Failure} Your input is invalid.")
                        .AsEphemeral(true)).ConfigureAwait(false);
                    return;
                }

                result = await this._shared.AddOrGetWordAsync(result).ConfigureAwait(false);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .AddEmbed(await this._shared.BuildWordEmbedAsync(result, context.Guild))).ConfigureAwait(false);
            }

            [SlashCommand("find", "Finds existing word in Luke Dictionary")]
            public async Task CmdFind(InteractionContext context, [Option("word", "Word to look for")] string word)
            {
                LukeWord result = await this._shared.GetWordAsync(word).ConfigureAwait(false);
                if (result == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent($"{ResponseEmoji.SeriousThonk} Nope, not found.")).ConfigureAwait(false);
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(await this._shared.BuildWordEmbedAsync(result, context.Guild))).ConfigureAwait(false);
                }
            }

            [SlashCommand("random", "Grabs a random word invented by Luke")]
            public async Task CmdRandom(InteractionContext context)
            {
                LukeWord result = await this._shared.GetRandomWordAsync().ConfigureAwait(false);
                if (result == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent($"{ResponseEmoji.SeriousThonk} Seems no word was added yet?")).ConfigureAwait(false);
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(await this._shared.BuildWordEmbedAsync(result, context.Guild))).ConfigureAwait(false);
                }
            }

            [SlashCommand("count", "Checks the amount of Luke's words accumulated so far")]
            public async Task CmdCount(InteractionContext context)
            {
                long count = await this._shared.GetWordsCountAsync().ConfigureAwait(false);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"I know of about {count} Luke words. {ResponseEmoji.EyesBlurry}")).ConfigureAwait(false);
            }

            [SlashCommand("list", "Gets a full list of all Luke's words!")]
            public async Task CmdList(InteractionContext context)
            {
                await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"Building Words List {ResponseEmoji.Parrot60fps}")).ConfigureAwait(false);
                try
                {
                    string result = await this._shared.GetWordsListAsync().ConfigureAwait(false);
                    await context.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"{ResponseEmoji.ParrotParty} {result} {ResponseEmoji.ParrotParty}"),
                        null).ConfigureAwait(false);
                }
                catch
                {
                    await context.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"{ResponseEmoji.FeelsBeanMan} I couldn't save the list to PasteMyst for some damn reason."),
                        null).ConfigureAwait(false);
                }
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
            // DSharpPlus commands handling is garbage (unlike Wolfringo's), and doesn't allow whitespaces - add a "mode" switch to this command to support "add" etc
            public Task CmdSwitch(CommandContext context, string mode, string word = null)
            {
                if (mode == null)
                    return this.CmdAdd(context, mode);

                switch (mode.Trim().ToLowerInvariant())
                {
                    case "add":
                        return this.CmdAdd(context, word);
                    case "find":
                        return this.CmdFind(context, word);
                    case "random":
                        return this.CmdRandom(context);
                    case "count":
                        return this.CmdCount(context);
                    case "list":
                        return this.CmdList(context);
                    // if none matched, use mode as the word itself
                    default:
                        return this.CmdAdd(context, mode);
                }
            }

            [Command("lukeadd")]
            public async Task CmdAdd(CommandContext context, string word)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    await context.RespondAsync($"{ResponseEmoji.Failure} And what's the word, huh?!").ConfigureAwait(false);
                    return;
                }

                DiscordUser user = context.Member ?? context.User;

                LukeWord result = new LukeWord(word, user.Id);
                result.GuildID = context.Guild?.Id;
                result.ChannelID = context.Channel?.Id;
                result.MessageID = context.Message?.Id;

                if (!this._shared.ValidateInput(result))
                {
                    await context.RespondAsync($"{ResponseEmoji.Failure} Your input is invalid.").ConfigureAwait(false);
                    return;
                }

                result = await this._shared.AddOrGetWordAsync(result).ConfigureAwait(false);
                await context.RespondAsync(await this._shared.BuildWordEmbedAsync(result, context.Guild)).ConfigureAwait(false);
            }

            private async Task CmdFind(CommandContext context, string word)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    await context.RespondAsync($"{ResponseEmoji.Failure} And what's the word, huh?!").ConfigureAwait(false);
                    return;
                }

                LukeWord result = await this._shared.GetWordAsync(word).ConfigureAwait(false);
                if (result == null)
                    await context.RespondAsync($"{ResponseEmoji.SeriousThonk} Nope, not found.").ConfigureAwait(false);
                else
                    await context.RespondAsync(await this._shared.BuildWordEmbedAsync(result, context.Guild)).ConfigureAwait(false);
            }

            private async Task CmdRandom(CommandContext context)
            {
                LukeWord result = await this._shared.GetRandomWordAsync().ConfigureAwait(false);
                if (result == null)
                    await context.RespondAsync($"{ResponseEmoji.SeriousThonk} Nope, not found.").ConfigureAwait(false);
                else
                    await context.RespondAsync(await this._shared.BuildWordEmbedAsync(result, context.Guild)).ConfigureAwait(false);
            }

            private async Task CmdCount(CommandContext context)
            {
                long count = await this._shared.GetWordsCountAsync().ConfigureAwait(false);
                await context.RespondAsync($"I know of about {count} Luke words. {ResponseEmoji.EyesBlurry}").ConfigureAwait(false);
            }

            private async Task CmdList(CommandContext context)
            {
                DiscordMessage response = await context.RespondAsync($"Building Words List {ResponseEmoji.Parrot60fps}").ConfigureAwait(false);
                try
                {
                    string result = await this._shared.GetWordsListAsync().ConfigureAwait(false);
                    await response.ModifyAsync($"{ResponseEmoji.ParrotParty} {result} {ResponseEmoji.ParrotParty}").ConfigureAwait(false);
                }
                catch
                {
                    await response.ModifyAsync($"{ResponseEmoji.FeelsBeanMan} I couldn't save the list to PasteMyst for some damn reason.").ConfigureAwait(false);
                }
            }
        }
    }
}
