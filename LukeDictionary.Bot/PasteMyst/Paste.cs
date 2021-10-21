using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace DevSubmarine.LukeDictionary.PasteMyst
{
    /// <summary>Represents a paste object.</summary>
    /// <remarks>This object is only partial, as not all features are currently needed.</remarks>
    /// <seealso href="https://paste.myst.rs/api-docs/objects"/>
    [DebuggerDisplay("PasteMyst Paste {Title}")]
    public class Paste
    {
        [JsonProperty("_id")]
        public string ID { get; }
        [JsonProperty("title")]
        public string Title { get; }
        [JsonProperty("expiresIn")]
        public PasteExpiration Expiration { get; init; }
        [JsonProperty("tags")]
        public ICollection<string> Tags { get; init; }
        [JsonProperty("pasties")]
        public IEnumerable<Pasty> Pasties { get; }

        public Paste(string title, IEnumerable<Pasty> pasties, PasteExpiration expiration = PasteExpiration.Never)
        {
            if (pasties?.Any() != true)
                throw new ArgumentException("At least one pasty is required", nameof(pasties));

            this.Title = title ?? string.Empty;
            this.Expiration = expiration;
            this.Pasties = pasties;
            this.Tags = new List<string>();
        }

        public Paste(string title, Pasty pasty, PasteExpiration expiration = PasteExpiration.Never)
            : this(title, new Pasty[] { pasty }, expiration) { }
        public Paste(IEnumerable<Pasty> pasties, PasteExpiration expiration = PasteExpiration.Never)
            : this(null, pasties, expiration) { }
        public Paste(Pasty pasty, PasteExpiration expiration = PasteExpiration.Never)
            : this(null, pasty, expiration) { }

        [JsonConstructor]
        private Paste(string _id, string title, IEnumerable<Pasty> pasties)
        {
            this.ID = _id;
            this.Title = title;
            this.Pasties = pasties;
        }
    }
}
