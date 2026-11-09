namespace DictionaryWebApp.Models
{
    // цифры для плиток статистики
    public class DictionaryStats
    {
        public int RecordsCount { get; set; }                 // записей в словаре
        public int EngWordsCount { get; set; }                // английских слов
        public int RusWordsCount { get; set; }                // русских слов
        public int TranslationPairsCount { get; set; }        // пар «слово — перевод»
        public int EngWordsWithSynonymsCount { get; set; }    // англ. слов с несколькими переводами
        public int RusWordsWithSynonymsCount { get; set; }    // рус. слов с несколькими переводами
    }
}
