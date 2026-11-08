namespace DictionaryWebApp.Models
{
    /// <summary>
    /// Сведения о направлении перевода (элемент обобщённого списка направлений).
    /// </summary>
    public class TranslationDirection
    {
        /// <summary>Режим работы, которому соответствует направление.</summary>
        public TranslationMode Mode { get; init; }

        /// <summary>Название направления, например «Англо-русский».</summary>
        public string Title { get; init; } = "";

        /// <summary>Язык исходного слова.</summary>
        public string SourceLanguage { get; init; } = "";

        /// <summary>Язык перевода.</summary>
        public string TargetLanguage { get; init; } = "";

        /// <summary>Количество слов, доступных для перевода в этом направлении.</summary>
        public int WordsCount { get; set; }

        /// <summary>Короткая подпись для меню, например «Англо → Русский».</summary>
        public string ShortTitle => $"{SourceLanguage} → {TargetLanguage}";
    }
}
