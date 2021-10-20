using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace DevSubmarine.LukeDictionary
{
    /// <summary>Represents Luke's misspelled word + metadata.</summary>
    public class LukeWord : IEquatable<LukeWord>, IEquatable<string>
    {
        [BsonId]
        public string Word { get; }
        [BsonElement("AddedBy")]
        public ulong AddedByUserID { get; }
        [BsonElement("CreationTime")]
        public DateTime CreationTimeUTC { get; }

        // additional metadata, in case needed in future?
        [BsonElement("GuildID")]
        public ulong GuildID { get; set; }
        [BsonElement("ChannelID")]
        public ulong ChannelID { get; set; }
        [BsonElement("MessageID")]
        public ulong MessageID { get; set; }

        [BsonConstructor(nameof(Word), nameof(AddedByUserID), nameof(CreationTimeUTC))]
        public LukeWord(string word, ulong addedByUserID, DateTime creationTimeUTC)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));

            // store as lowercase - case does not matter, while storing as lowercase reduces the need for complex indexing
            this.Word = word.Trim().ToLowerInvariant();
            this.AddedByUserID = addedByUserID;
            this.CreationTimeUTC = creationTimeUTC;
        }

        public LukeWord(string word, ulong addedByUserID)
            : this(word, addedByUserID, DateTime.UtcNow) { }

        /// <inheritdoc/>
        public override string ToString()
            => this.Word;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is LukeWord wordObject)
                return this.Equals(wordObject);
            if (obj is string wordString)
                return this.Equals(wordString);
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(LukeWord other)
            => other != null && this.Word.Equals(other.Word, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public bool Equals(string other)
            => other != null && this.Word.Equals(other, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override int GetHashCode()
            => this.Word.GetHashCode(StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public static bool operator ==(LukeWord left, LukeWord right)
            => EqualityComparer<LukeWord>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(LukeWord left, LukeWord right)
            => !(left == right);
    }
}
