namespace DictionaryWebApp.Models
{
    /// <summary>
    /// Сведения о синонимичных переводах
    /// (элемент обобщённого списка слов, имеющих несколько переводов).
    /// </summary>
    public class SynonymInfo
    {
        /// <summary>Исходное слово.</summary>
        public string Word { get; init; } = "";

        /// <summary>Направление перевода, в котором у слова есть синонимы.</summary>
        public TranslationMode Direction { get; init; }

        /// <summary>Синонимичные переводы слова.</summary>
        public List<string> Synonyms { get; init; } = new();
    }
}
