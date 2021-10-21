using System.Diagnostics;
using Newtonsoft.Json;

namespace DevSubmarine.LukeDictionary.PasteMyst
{
    /// <summary>Represents a pasty object.</summary>
    /// <seealso href="https://paste.myst.rs/api-docs/objects"/>
    [DebuggerDisplay("PasteMyst Pasty {Title}")]
    public class Pasty
    {
        [JsonProperty("_id")]
        public string ID { get; }
        [JsonProperty("language")]
        public string Language { get; }
        [JsonProperty("title")]
        public string Title { get; }
        [JsonProperty("code")]
        public string Content { get; }

        public Pasty(string content, string title, string language)
        {
            this.Content = content;
            this.Title = title ?? string.Empty;
            this.Language = language;
        }

        public Pasty(string content, string title)
            : this(content, title, PastyLanguages.PlainText) { }
        public Pasty(string content)
            : this(content, null, PastyLanguages.PlainText) { }

        [JsonConstructor]
        private Pasty(string _id, string code, string title, string language) 
            : this(code, title, language)
        {
            this.ID = _id;
        }
    }
}
