using System;
using MongoDB.Bson.Serialization.Attributes;

namespace DevSubmarine.LukeDictionary
{
    /// <summary>Represents Luke's misspelled word + metadata.</summary>
    public class LukeWord
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

            this.Word = word;
            this.AddedByUserID = addedByUserID;
            this.CreationTimeUTC = creationTimeUTC;
        }
    }
}
