namespace DictionaryWebApp.Models
{
    /// <summary>
    /// Статистика словаря.
    /// </summary>
    public class DictionaryStats
    {
        /// <summary>Общее количество записей в словаре (строк «слово:переводы»).</summary>
        public int RecordsCount { get; set; }

        /// <summary>Количество английских слов.</summary>
        public int EngWordsCount { get; set; }

        /// <summary>Количество русских слов.</summary>
        public int RusWordsCount { get; set; }

        /// <summary>Общее количество пар «слово — перевод».</summary>
        public int TranslationPairsCount { get; set; }

        /// <summary>Количество английских слов с несколькими переводами.</summary>
        public int EngWordsWithSynonymsCount { get; set; }

        /// <summary>Количество русских слов с несколькими переводами.</summary>
        public int RusWordsWithSynonymsCount { get; set; }
    }
}
