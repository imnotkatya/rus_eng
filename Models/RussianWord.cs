namespace DictionaryWebApp.Models
{
    // русское слово и его английские переводы
    public class RussianWord
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
