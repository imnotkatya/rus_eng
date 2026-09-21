using System.Text;
using DictionaryWebApp.Models;

namespace DictionaryWebApp.Services
{
    /// <summary>
    /// Реализация сервиса словаря.
    /// База данных хранится в виде двух контейнеров типа map:
    ///   - _engToRus — англо-русский словарь;
    ///   - _rusToEng — русско-английский словарь.
    /// В C# аналогом std::map (C++) является SortedDictionary.
    /// </summary>
    public class DictionaryService : IDictionaryService
    {
        // ============================================================
        //  БАЗА ДАННЫХ СЛОВАРЯ — ДВА КОНТЕЙНЕРА ТИПА MAP
        // ============================================================

        /// <summary>Контейнер №1: англо-русский словарь (англ. слово → список рус. переводов).</summary>
        private readonly SortedDictionary<string, List<string>> _engToRus = new();

        /// <summary>Контейнер №2: русско-английский словарь (рус. слово → список англ. переводов).</summary>
        private readonly SortedDictionary<string, List<string>> _rusToEng = new();

        /// <summary>Путь к файлу с базой данных словаря.</summary>
        private readonly string _filePath;

        /// <summary>Объект для синхронизации доступа к словарям.</summary>
        private readonly object _lock = new();

        /// <summary>
        /// Создать сервис словаря.
        /// </summary>
        /// <param name="filePath">Путь к файлу dictionary.txt.</param>
        public DictionaryService(string filePath)
        {
            _filePath = filePath;
            LoadFromFile();
        }

        // ============================================================
        //  ЗАГРУЗКА ИЗ ФАЙЛА
        // ============================================================

        /// <summary>
        /// Загрузить базу данных словаря из файла.
        /// Формат строки: английское_слово:русский_перевод1,русский_перевод2,...
        /// </summary>
        public void LoadFromFile()
        {
            lock (_lock)
            {
                // Очищаем оба контейнера перед загрузкой
                _engToRus.Clear();
                _rusToEng.Clear();

                // Если файла нет — ничего не делаем (словарь будет пустым)
                if (!File.Exists(_filePath))
                    return;

                // Читаем файл построчно в кодировке UTF-8
                foreach (string line in File.ReadLines(_filePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Разделяем строку на английское слово и русские переводы
                    int colonPos = line.IndexOf(':');
                    if (colonPos < 0) continue;

                    string engWord = line.Substring(0, colonPos).Trim();
                    string rusPart = line.Substring(colonPos + 1).Trim();

                    if (engWord.Length == 0 || rusPart.Length == 0) continue;

                    // Разбиваем русскую часть по запятым
                    string[] rusWords = rusPart.Split(
                        new[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries);

                    var translations = new List<string>();
                    foreach (string rw in rusWords)
                    {
                        string trimmed = rw.Trim();
                        if (trimmed.Length > 0)
                            translations.Add(trimmed);
                    }

                    if (translations.Count == 0) continue;

                    // Заполняем контейнер №1 (англо-русский)
                    _engToRus[engWord] = translations;

                    // Заполняем контейнер №2 (русско-английский)
                    // Каждый русский перевод становится отдельным ключом
                    foreach (string rusWord in translations)
                    {
                        if (!_rusToEng.ContainsKey(rusWord))
                            _rusToEng[rusWord] = new List<string>();

                        if (!_rusToEng[rusWord].Contains(engWord))
                            _rusToEng[rusWord].Add(engWord);
                    }
                }
            }
        }

        // ============================================================
        //  ПЕРЕВОД
        // ============================================================

        /// <summary>
        /// Найти перевод английского слова.
        /// Возвращает список русских переводов (синонимов) или пустой список.
        /// </summary>
        public List<string> TranslateEngToRus(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new List<string>();

            return _engToRus.TryGetValue(word.Trim(), out var translations)
                ? translations
                : new List<string>();
        }

        /// <summary>
        /// Найти перевод русского слова.
        /// Возвращает список английских переводов (синонимов) или пустой список.
        /// </summary>
        public List<string> TranslateRusToEng(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new List<string>();

            return _rusToEng.TryGetValue(word.Trim(), out var translations)
                ? translations
                : new List<string>();
        }

        // ============================================================
        //  ПОЛУЧЕНИЕ ВСЕГО СЛОВАРЯ
        // ============================================================

        /// <summary>
        /// Получить весь англо-русский словарь.
        /// Благодаря SortedDictionary, ключи уже отсортированы по алфавиту.
        /// </summary>
        public SortedDictionary<string, List<string>> GetAllEngToRus()
        {
            // Возвращаем копию, чтобы внешний код не мог изменить оригинал
            return new SortedDictionary<string, List<string>>(_engToRus);
        }

        /// <summary>
        /// Получить весь русско-английский словарь.
        /// Благодаря SortedDictionary, ключи уже отсортированы по алфавиту.
        /// </summary>
        public SortedDictionary<string, List<string>> GetAllRusToEng()
        {
            return new SortedDictionary<string, List<string>>(_rusToEng);
        }

        // ============================================================
        //  СЛОВА С СИНОНИМАМИ
        // ============================================================

        /// <summary>
        /// Получить слова, у которых больше одного перевода.
        /// </summary>
        public List<KeyValuePair<string, List<string>>> GetWordsWithSynonyms()
        {
            var result = new List<KeyValuePair<string, List<string>>>();

            foreach (var pair in _engToRus)
            {
                if (pair.Value.Count > 1)
                    result.Add(pair);
            }

            return result;
        }

        // ============================================================
        //  СТАТИСТИКА
        // ============================================================

        /// <summary>
        /// Получить статистику по словарю.
        /// </summary>
        public DictionaryStats GetStats()
        {
            int withSynonyms = 0;
            foreach (var pair in _engToRus)
            {
                if (pair.Value.Count > 1)
                    withSynonyms++;
            }

            return new DictionaryStats
            {
                EngWordsCount = _engToRus.Count,
                RusWordsCount = _rusToEng.Count,
                WordsWithSynonymsCount = withSynonyms
            };
        }
    }
}