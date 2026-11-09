namespace DictionaryWebApp.Models
{
    // направление перевода (их два: англо-русский и русско-английский)
    public class TranslationDirection
    {
        public TranslationMode Mode { get; set; }
        public string Title { get; set; } = "";            // например «Англо-русский»
        public string SourceLanguage { get; set; } = "";   // с какого языка
        public string TargetLanguage { get; set; } = "";   // на какой язык
        public int WordsCount { get; set; }                // сколько слов в этом направлении

        // короткая подпись, например «Англо → Русский»
        public string ShortTitle
        {
            get { return SourceLanguage + " → " + TargetLanguage; }
        }
    }
}
