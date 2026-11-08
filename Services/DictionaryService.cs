using System.Globalization;
using System.Text;
using DictionaryWebApp.Models;

namespace DictionaryWebApp.Services
{
    /// <summary>
    /// Реализация сервиса словаря.
    /// «База данных» словаря хранится в виде двух контейнеров типа map:
    ///   - _engToRus — англо-русский словарь;
    ///   - _rusToEng — русско-английский словарь.
    /// В C# аналогом std::map (C++) является SortedDictionary —
    /// ключи в нём всегда упорядочены по алфавиту.
    /// На основе контейнеров формируются обобщённые списки (List&lt;T&gt;):
    /// английских слов, русских слов, синонимичных переводов и направлений перевода.
    /// </summary>
    public class DictionaryService : IDictionaryService
    {
        // ============================================================
        //  БАЗА ДАННЫХ СЛОВАРЯ — ДВА КОНТЕЙНЕРА ТИПА MAP
        // ============================================================

        /// <summary>Сравнение строк по алфавиту без учёта регистра (Apple = apple).</summary>
        private static readonly StringComparer WordComparer = CreateComparer();

        /// <summary>Контейнер №1: англо-русский словарь (англ. слово → список рус. переводов).</summary>
        private SortedDictionary<string, List<string>> _engToRus = new(WordComparer);

        /// <summary>Контейнер №2: русско-английский словарь (рус. слово → список англ. переводов).</summary>
        private SortedDictionary<string, List<string>> _rusToEng = new(WordComparer);

        // ============================================================
        //  ОБОБЩЁННЫЕ СПИСКИ
        // ============================================================

        /// <summary>Список №1: сведения об английских словах.</summary>
        private List<EnglishWord> _englishWords = new();

        /// <summary>Список №2: сведения о русских словах.</summary>
        private List<RussianWord> _russianWords = new();

        /// <summary>Список №3: сведения о синонимичных переводах.</summary>
        private List<SynonymInfo> _synonyms = new();

        /// <summary>Список №4: сведения о направлениях перевода.</summary>
        private readonly List<TranslationDirection> _directions = new()
        {
            new TranslationDirection
            {
                Mode = TranslationMode.EngToRus,
                Title = "Англо-русский",
                SourceLanguage = "Англо",
                TargetLanguage = "Русский"
            },
            new TranslationDirection
            {
                Mode = TranslationMode.RusToEng,
                Title = "Русско-английский",
                SourceLanguage = "Русский",
                TargetLanguage = "Английский"
            }
        };

        /// <summary>Путь к файлу словаря по умолчанию.</summary>
        private readonly string _filePath;

        /// <summary>Объект для синхронизации доступа к словарю.</summary>
        private readonly object _lock = new();

        /// <summary>Результат последней загрузки.</summary>
        public LoadResult LastLoad { get; private set; } = new();

        /// <summary>
        /// Создать сервис словаря и загрузить словарь из файла по умолчанию.
        /// </summary>
        /// <param name="filePath">Путь к файлу dictionary.txt.</param>
        public DictionaryService(string filePath)
        {
            _filePath = filePath;
            LoadFromFile();
        }

        private static StringComparer CreateComparer()
        {
            try
            {
                return StringComparer.Create(CultureInfo.GetCultureInfo("ru-RU"), ignoreCase: true);
            }
            catch (CultureNotFoundException)
            {
                return StringComparer.OrdinalIgnoreCase;
            }
        }

        // ============================================================
        //  ЗАГРУЗКА ИЗ ФАЙЛА
        // ============================================================

        /// <summary>
        /// Загрузить «базу данных» словаря из файла по умолчанию.
        /// </summary>
        public LoadResult LoadFromFile()
        {
            string fileName = Path.GetFileName(_filePath);

            if (!File.Exists(_filePath))
            {
                var result = new LoadResult { FileName = fileName };
                result.Errors.Add($"Файл «{fileName}» не найден.");
                lock (_lock) LastLoad = result;
                return result;
            }

            using var stream = File.OpenRead(_filePath);
            return LoadFromStream(stream, fileName);
        }

        /// <summary>
        /// Загрузить «базу данных» словаря из потока (файла, выбранного пользователем).
        /// Формат строки: английское_слово:русский_перевод1,русский_перевод2,...
        /// Строки с ошибками пропускаются, их номера попадают в список ошибок.
        /// Если в файле нет ни одной правильной записи, текущий словарь не меняется.
        /// </summary>
        public LoadResult LoadFromStream(Stream stream, string fileName)
        {
            var result = new LoadResult { FileName = fileName };

            // Заполняем новые контейнеры, чтобы при ошибке не испортить текущий словарь
            var engToRus = new SortedDictionary<string, List<string>>(WordComparer);
            var rusToEng = new SortedDictionary<string, List<string>>(WordComparer);

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string? line;
                int lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string? error = ParseLine(line, engToRus, rusToEng);
                    if (error != null)
                    {
                        result.SkippedLines++;
                        result.Errors.Add($"Строка {lineNumber}: {error}");
                    }
                }
            }

            if (engToRus.Count == 0)
            {
                result.Errors.Insert(0, "В файле нет ни одной правильной записи. Словарь не изменён.");
                lock (_lock)
                {
                    // Если словарь ещё ни разу не загружался — запоминаем неудачную попытку
                    if (!LastLoad.Success) LastLoad = result;
                }
                return result;
            }

            lock (_lock)
            {
                _engToRus = engToRus;
                _rusToEng = rusToEng;
                BuildLists();

                result.Success = true;
                result.LoadedRecords = _engToRus.Count;
                LastLoad = result;
            }

            return result;
        }

        /// <summary>
        /// Разобрать одну строку файла и добавить её в оба контейнера.
        /// Возвращает текст ошибки или null, если строка правильная.
        /// </summary>
        private static string? ParseLine(
            string line,
            SortedDictionary<string, List<string>> engToRus,
            SortedDictionary<string, List<string>> rusToEng)
        {
            int colonPos = line.IndexOf(':');
            if (colonPos < 0)
                return "нет двоеточия (ожидается формат «word:перевод1,перевод2»).";

            string engWord = line.Substring(0, colonPos).Trim().ToLowerInvariant();
            string rusPart = line.Substring(colonPos + 1).Trim();

            if (!WordValidator.IsEnglish(engWord))
                return $"«{engWord}» не является английским словом.";

            var translations = new List<string>();
            foreach (string part in rusPart.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                string rusWord = part.Trim().ToLowerInvariant();
                if (!WordValidator.IsRussian(rusWord))
                    return $"«{rusWord}» не является русским словом.";

                if (!translations.Contains(rusWord, WordComparer))
                    translations.Add(rusWord);
            }

            if (translations.Count == 0)
                return $"у слова «{engWord}» нет перевода.";

            // Контейнер №1 (англо-русский). Если слово повторяется — объединяем переводы
            if (!engToRus.TryGetValue(engWord, out var existing))
            {
                existing = new List<string>();
                engToRus[engWord] = existing;
            }
            foreach (string rusWord in translations)
                if (!existing.Contains(rusWord, WordComparer))
                    existing.Add(rusWord);

            // Контейнер №2 (русско-английский): каждый русский перевод становится ключом
            foreach (string rusWord in translations)
            {
                if (!rusToEng.TryGetValue(rusWord, out var engList))
                {
                    engList = new List<string>();
                    rusToEng[rusWord] = engList;
                }
                if (!engList.Contains(engWord, WordComparer))
                    engList.Add(engWord);
            }

            return null;
        }

        /// <summary>
        /// Сформировать обобщённые списки по содержимому контейнеров.
        /// Вызывается под блокировкой после каждой загрузки.
        /// </summary>
        private void BuildLists()
        {
            _englishWords = _engToRus
                .Select(p => new EnglishWord { Word = p.Key, Translations = new List<string>(p.Value) })
                .ToList();

            _russianWords = _rusToEng
                .Select(p => new RussianWord { Word = p.Key, Translations = new List<string>(p.Value) })
                .ToList();

            _synonyms = new List<SynonymInfo>();
            foreach (var w in _englishWords.Where(w => w.HasSynonyms))
                _synonyms.Add(new SynonymInfo { Word = w.Word, Direction = TranslationMode.EngToRus, Synonyms = w.Translations });
            foreach (var w in _russianWords.Where(w => w.HasSynonyms))
                _synonyms.Add(new SynonymInfo { Word = w.Word, Direction = TranslationMode.RusToEng, Synonyms = w.Translations });

            foreach (var d in _directions)
                d.WordsCount = d.Mode == TranslationMode.EngToRus ? _engToRus.Count : _rusToEng.Count;
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

            lock (_lock)
            {
                return _engToRus.TryGetValue(word.Trim(), out var translations)
                    ? new List<string>(translations)
                    : new List<string>();
            }
        }

        /// <summary>
        /// Найти перевод русского слова.
        /// Возвращает список английских переводов (синонимов) или пустой список.
        /// </summary>
        public List<string> TranslateRusToEng(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new List<string>();

            lock (_lock)
            {
                return _rusToEng.TryGetValue(word.Trim(), out var translations)
                    ? new List<string>(translations)
                    : new List<string>();
            }
        }

        /// <summary>Перевод слова в заданном режиме работы.</summary>
        public List<string> Translate(string word, TranslationMode mode) =>
            mode == TranslationMode.EngToRus
                ? TranslateEngToRus(word)
                : TranslateRusToEng(word);

        // ============================================================
        //  ОБОБЩЁННЫЕ СПИСКИ (ЗАПРОСЫ)
        // ============================================================

        /// <summary>
        /// Список английских слов с русскими переводами.
        /// ascending = true — по алфавиту (A→Z), false — в обратном порядке (Z→A).
        /// </summary>
        public List<EnglishWord> GetEnglishWords(bool ascending = true)
        {
            lock (_lock)
            {
                var list = new List<EnglishWord>(_englishWords);
                if (!ascending) list.Reverse();
                return list;
            }
        }

        /// <summary>
        /// Список русских слов с английскими переводами.
        /// ascending = true — по алфавиту (А→Я), false — в обратном порядке (Я→А).
        /// </summary>
        public List<RussianWord> GetRussianWords(bool ascending = true)
        {
            lock (_lock)
            {
                var list = new List<RussianWord>(_russianWords);
                if (!ascending) list.Reverse();
                return list;
            }
        }

        /// <summary>
        /// Слова, имеющие несколько синонимичных переводов.
        /// direction = null — в обоих направлениях.
        /// </summary>
        public List<SynonymInfo> GetSynonyms(TranslationMode? direction = null)
        {
            lock (_lock)
            {
                return _synonyms
                    .Where(s => direction == null || s.Direction == direction)
                    .ToList();
            }
        }

        /// <summary>Список направлений перевода.</summary>
        public List<TranslationDirection> GetDirections()
        {
            lock (_lock)
            {
                return new List<TranslationDirection>(_directions);
            }
        }

        // ============================================================
        //  СТАТИСТИКА
        // ============================================================

        /// <summary>
        /// Статистика словаря, в том числе общее количество записей.
        /// </summary>
        public DictionaryStats GetStats()
        {
            lock (_lock)
            {
                return new DictionaryStats
                {
                    RecordsCount = _engToRus.Count,
                    EngWordsCount = _engToRus.Count,
                    RusWordsCount = _rusToEng.Count,
                    TranslationPairsCount = _engToRus.Sum(p => p.Value.Count),
                    EngWordsWithSynonymsCount = _englishWords.Count(w => w.HasSynonyms),
                    RusWordsWithSynonymsCount = _russianWords.Count(w => w.HasSynonyms)
                };
            }
        }
    }
}
