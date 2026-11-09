namespace DictionaryWebApp.Models
{
    // английское слово и его русские переводы
    public class EnglishWord
    {
        public string Word { get; set; } = "";
        public List<string> Translations { get; set; } = new List<string>();

        // true, если переводов больше одного
        public bool HasSynonyms
        {
            get { return Translations.Count > 1; }
        }
    }
}
