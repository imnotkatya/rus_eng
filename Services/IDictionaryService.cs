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
        // ========== Загрузка из файла ==========

        /// <summary>Загрузить базу данных словаря из файла.</summary>
        void LoadFromFile();

        // ========== Перевод ==========

        /// <summary>Перевод английского слова на русский.</summary>
        List<string> TranslateEngToRus(string word);

        /// <summary>Перевод русского слова на английский.</summary>
        List<string> TranslateRusToEng(string word);

        // ========== Получение всего словаря (для сортировки) ==========

        /// <summary>Получить весь англо-русский словарь, отсортированный по алфавиту.</summary>
        SortedDictionary<string, List<string>> GetAllEngToRus();

        /// <summary>Получить весь русско-английский словарь, отсортированный по алфавиту.</summary>
        SortedDictionary<string, List<string>> GetAllRusToEng();

        // ========== Слова с синонимами ==========

        /// <summary>Получить слова, у которых больше одного перевода.</summary>
        List<KeyValuePair<string, List<string>>> GetWordsWithSynonyms();

        // ========== Статистика ==========

        /// <summary>Получить статистику по словарю.</summary>
        DictionaryStats GetStats();
    }
}