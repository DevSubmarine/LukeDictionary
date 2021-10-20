namespace DevSubmarine.LukeDictionary.Discord
{
    /// <summary>Options for Discord related functionalities.</summary>
    public class DiscordOptions
    {
        /// <summary>The bot's secret token.</summary>
        public string BotToken { get; set; }
        /// <summary>Guild ID to register commands in. Null to register globally.</summary>
        public ulong? GuildID { get; set; } = 441702024715960330;

        // for non-slash commands
        public string Prefix { get; set; } = "!";
        public bool AcceptMentionPrefix { get; set; } = true;
        public bool AcceptBotMessages { get; set; } = false;
        public bool RequirePublicMessagePrefix { get; set; } = true;
        public bool RequirePrivateMessagePrefix { get; set; } = false;
    }
}
