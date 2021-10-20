namespace DevSubmarine.LukeDictionary.Discord
{
    /// <summary>Options for Discord related functionalities.</summary>
    public class DiscordOptions
    {
        /// <summary>The bot's secret token.</summary>
        public string BotToken { get; set; }
        /// <summary>Guild ID to register commands in. Null to register globally.</summary>
        public ulong? GuildID { get; set; } = 441702024715960330;
    }
}
