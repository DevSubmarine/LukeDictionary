using DevSubmarine.LukeDictionary.PasteMyst;

namespace DevSubmarine.LukeDictionary.Words
{
    public class LukeWordsOptions
    {
        /// <summary>How long the words list paste will be alive.</summary>
        public PasteExpiration ListExpiration { get; set; } = PasteExpiration.OneMonth;
    }
}
