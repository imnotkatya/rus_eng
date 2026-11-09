namespace DictionaryWebApp.Models
{
    // слово, у которого несколько переводов
    public class SynonymInfo
    {
        public string Word { get; set; } = "";
        public TranslationMode Direction { get; set; }           // в каком направлении
        public List<string> Synonyms { get; set; } = new List<string>();
    }
}
