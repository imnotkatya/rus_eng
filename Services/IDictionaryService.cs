using DictionaryWebApp.Models;

namespace DictionaryWebApp.Services
{
    /// <summary>
    /// Интерфейс сервиса словаря.
    /// Определяет набор методов для работы с англо-русским
    /// и русско-английским словарём.
    /// </summary>
    public interface IDictionaryService
    {
        // ========== Загрузка «базы данных» из файла ==========

        /// <summary>Загрузить словарь из файла по умолчанию (Data/dictionary.txt).</summary>
        LoadResult LoadFromFile();

        /// <summary>Загрузить словарь из файла, выбранного пользователем.</summary>
        LoadResult LoadFromStream(Stream stream, string fileName);

        /// <summary>Результат последней загрузки.</summary>
        LoadResult LastLoad { get; }

        // ========== Перевод ==========

        /// <summary>Перевод английского слова на русский (список переводов).</summary>
        List<string> TranslateEngToRus(string word);

        /// <summary>Перевод русского слова на английский (список переводов).</summary>
        List<string> TranslateRusToEng(string word);

        /// <summary>Перевод слова в заданном режиме.</summary>
        List<string> Translate(string word, TranslationMode mode);

        // ========== Обобщённые списки ==========

        /// <summary>Сведения об английских словах (по алфавиту или в обратном порядке).</summary>
        List<EnglishWord> GetEnglishWords(bool ascending = true);

        /// <summary>Сведения о русских словах (по алфавиту или в обратном порядке).</summary>
        List<RussianWord> GetRussianWords(bool ascending = true);

        /// <summary>Сведения о синонимичных переводах (null — оба направления).</summary>
        List<SynonymInfo> GetSynonyms(TranslationMode? direction = null);

        /// <summary>Сведения о направлениях перевода.</summary>
        List<TranslationDirection> GetDirections();

        // ========== Статистика ==========

        /// <summary>Статистика словаря, в т.ч. общее количество записей.</summary>
        DictionaryStats GetStats();
    }
}
