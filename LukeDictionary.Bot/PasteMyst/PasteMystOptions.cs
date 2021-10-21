namespace DevSubmarine.LukeDictionary.PasteMyst
{
    public class PasteMystOptions
    {
        public string UserAgent { get; set; } = $"DevSubmarine's LukeDictionary v{AppVersion.Version}";
        public string AuthorizationToken { get; set; } = null;
    }
}
