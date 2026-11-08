namespace DictionaryWebApp.Models
{
    /// <summary>
    /// Сведения об английском слове (элемент обобщённого списка английских слов).
    /// </summary>
    public class EnglishWord
    {
        /// <summary>Английское слово.</summary>
        public string Word { get; init; } = "";

        /// <summary>Русские переводы слова (синонимичные переводы).</summary>
        public List<string> Translations { get; init; } = new();

        /// <summary>Имеет ли слово несколько синонимичных переводов.</summary>
        public bool HasSynonyms => Translations.Count > 1;
    }
}
