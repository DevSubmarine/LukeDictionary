namespace DevSubmarine.LukeDictionary.Discord
{
    public static class ResponseEmoji
    {
        public static string Success => "\u2705";
        public static string Failure => "\u274C";

        // custom emotes work everywhere when used as slash command
        // but when used in simple command, they'll work only in the server they come from
        public static string SeriousThonk => "<:SeriousThonk:526806403935895562>";
        public static string EyesBlurry => "<:eyes_blurry:529187668064469002>";
        public static string JerryWhat => "<:what:526104145728503808>";
    }
}
