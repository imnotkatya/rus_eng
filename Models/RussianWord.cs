namespace DictionaryWebApp.Models
{
    /// <summary>
    /// Сведения о русском слове (элемент обобщённого списка русских слов).
    /// </summary>
    public class RussianWord
    {
        /// <summary>Русское слово.</summary>
        public string Word { get; init; } = "";

        /// <summary>Английские переводы слова (синонимичные переводы).</summary>
        public List<string> Translations { get; init; } = new();

        /// <summary>Имеет ли слово несколько синонимичных переводов.</summary>
        public bool HasSynonyms => Translations.Count > 1;
    }
}
