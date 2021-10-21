namespace DevSubmarine.LukeDictionary.Discord
{
    /// <summary>Options for Discord related functionalities.</summary>
    public class DiscordOptions
    {
        /// <summary>The bot's secret token.</summary>
        public string BotToken { get; set; }

        // for slash commands
        /// <summary>Whether slash commands should get registered. Defaults to true.</summary>
        public bool EnableSlashCommands { get; set; } = true;
        /// <summary>Guild ID to register commands in. Null to register globally.</summary>
        public ulong? GuildID { get; set; } = 441702024715960330;

        // for simple commands
        /// <summary>Whether simple commands should get registered. Defaults to true.</summary>
        /// <remarks>Simple commands means one that use text prefix.</remarks>
        public bool EnableSimpleCommands { get; set; } = true;
        /// <summary>Text prefix for simple commands. Defaults to `!`.</summary>
        public string Prefix { get; set; } = "!";
        /// <summary>Whether mention should be accepted instead of prefix. Defaults to true.</summary>
        public bool AcceptMentionPrefix { get; set; } = true;
        /// <summary>Whether messages from other bots should be ignored. Defaults to true.</summary>
        public bool IgnoreBotMessages { get; set; } = true;
        /// <summary>Whether <see cref="Prefix"/> is required when in public (server) context. Defaults to true.</summary>
        public bool RequirePublicMessagePrefix { get; set; } = true;
        /// <summary>Whether <see cref="Prefix"/> is required when in private (DM) context. Defaults to false.</summary>
        public bool RequirePrivateMessagePrefix { get; set; } = false;
    }
}
