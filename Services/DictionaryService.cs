using System.Globalization;
using System.Text;
using DictionaryWebApp.Models;

namespace DictionaryWebApp.Services
{
    // Тут вся логика словаря: загрузка из файла, перевод, списки и статистика.
    // Словарь храню в двух контейнерах map (в C# это SortedDictionary),
    // они сами держат слова по алфавиту.
    public class DictionaryService : IDictionaryService
    {
        // сравниваю слова по русскому алфавиту и без учёта регистра (Book = book)
        private static StringComparer comparer = StringComparer.Create(new CultureInfo("ru-RU"), true);

        // два контейнера: book -> [книга, том, издание] и том -> [book, volume]
        private SortedDictionary<string, List<string>> engToRus = new SortedDictionary<string, List<string>>(comparer);
        private SortedDictionary<string, List<string>> rusToEng = new SortedDictionary<string, List<string>>(comparer);

        // обобщённые списки (по заданию)
        private List<EnglishWord> englishWords = new List<EnglishWord>();
        private List<RussianWord> russianWords = new List<RussianWord>();
        private List<SynonymInfo> synonyms = new List<SynonymInfo>();
        private List<TranslationDirection> directions = new List<TranslationDirection>();

        // путь к Data/dictionary.txt
        private string filePath;

        // чем закончилась последняя загрузка (показываю на странице Загрузка)
        public LoadResult LastLoad { get; set; } = new LoadResult();

        public DictionaryService(string path)
        {
            filePath = path;

            // направлений всего два, заполняю сразу
            TranslationDirection engRus = new TranslationDirection();
            engRus.Mode = TranslationMode.EngToRus;
            engRus.Title = "Англо-русский";
            engRus.SourceLanguage = "Англо";
            engRus.TargetLanguage = "Русский";
            directions.Add(engRus);

            TranslationDirection rusEng = new TranslationDirection();
            rusEng.Mode = TranslationMode.RusToEng;
            rusEng.Title = "Русско-английский";
            rusEng.SourceLanguage = "Русский";
            rusEng.TargetLanguage = "Английский";
            directions.Add(rusEng);

            // при запуске сразу загружаю словарь
            LoadFromFile();
        }

        // ---------- загрузка ----------

        // загрузка из файла по умолчанию
        public LoadResult LoadFromFile()
        {
            string fileName = Path.GetFileName(filePath);

            if (!File.Exists(filePath))
            {
                LastLoad = new LoadResult();
                LastLoad.FileName = fileName;
                LastLoad.Errors.Add("Файл «" + fileName + "» не найден.");
                return LastLoad;
            }

            using (FileStream stream = File.OpenRead(filePath))
            {
                return LoadFromStream(stream, fileName);
            }
        }

        // загрузка из любого файла (и по умолчанию, и выбранного пользователем)
        public LoadResult LoadFromStream(Stream stream, string fileName)
        {
            LoadResult result = new LoadResult();
            result.FileName = fileName;

            // собираю во временные контейнеры, чтобы плохой файл не испортил рабочий словарь
            SortedDictionary<string, List<string>> newEngToRus = new SortedDictionary<string, List<string>>(comparer);
            SortedDictionary<string, List<string>> newRusToEng = new SortedDictionary<string, List<string>>(comparer);

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                int lineNumber = 0;
                string? line = reader.ReadLine();

                // читаю файл построчно, пока строки не закончатся
                while (line != null)
                {
                    lineNumber++;

                    if (line.Trim() != "")   // пустые строки пропускаю
                    {
                        string? error = ParseLine(line, newEngToRus, newRusToEng);
                        if (error != null)
                        {
                            result.SkippedLines++;
                            result.Errors.Add("Строка " + lineNumber + ": " + error);
                        }
                    }

                    line = reader.ReadLine();
                }
            }

            // ни одной нормальной строки — старый словарь не трогаю
            if (newEngToRus.Count == 0)
            {
                result.Errors.Insert(0, "В файле нет ни одной правильной записи. Словарь не изменён.");
                if (engToRus.Count == 0)   // если словаря ещё не было, запоминаю эту попытку
                    LastLoad = result;
                return result;
            }

            // всё хорошо — заменяю словарь новым и пересобираю списки
            engToRus = newEngToRus;
            rusToEng = newRusToEng;
            BuildLists();

            result.Success = true;
            result.LoadedRecords = engToRus.Count;
            LastLoad = result;
            return result;
        }

        // разбор одной строки, например "book:книга,том,издание"
        // возвращает текст ошибки или null, если строка правильная
        private string? ParseLine(string line,
            SortedDictionary<string, List<string>> engDict,
            SortedDictionary<string, List<string>> rusDict)
        {
            int colon = line.IndexOf(':');
            if (colon < 0)
                return "нет двоеточия (ожидается формат «word:перевод1,перевод2»).";

            // слева от двоеточия английское слово
            string engWord = line.Substring(0, colon).Trim().ToLower();
            if (!WordValidator.IsEnglish(engWord))
                return "«" + engWord + "» не является английским словом.";

            // справа переводы через запятую
            string[] parts = line.Substring(colon + 1).Split(',');
            List<string> translations = new List<string>();

            foreach (string part in parts)
            {
                string rusWord = part.Trim().ToLower();
                if (rusWord == "")
                    continue;   // лишняя запятая, например "кот,,кошка"

                if (!WordValidator.IsRussian(rusWord))
                    return "«" + rusWord + "» не является русским словом.";

                if (!translations.Contains(rusWord))
                    translations.Add(rusWord);
            }

            if (translations.Count == 0)
                return "у слова «" + engWord + "» нет перевода.";

            // кладу в оба контейнера: book -> книга и сразу обратно книга -> book
            foreach (string rusWord in translations)
            {
                AddPair(engDict, engWord, rusWord);
                AddPair(rusDict, rusWord, engWord);
            }
            return null;
        }

        // добавить перевод к слову; если слова ещё нет — создаю для него пустой список
        private void AddPair(SortedDictionary<string, List<string>> dict, string word, string translation)
        {
            if (!dict.ContainsKey(word))
                dict[word] = new List<string>();

            if (!dict[word].Contains(translation))
                dict[word].Add(translation);
        }

        // собираю обобщённые списки из контейнеров (после каждой загрузки)
        private void BuildLists()
        {
            englishWords = new List<EnglishWord>();
            foreach (string word in engToRus.Keys)
            {
                EnglishWord w = new EnglishWord();
                w.Word = word;
                w.Translations = engToRus[word];
                englishWords.Add(w);
            }

            russianWords = new List<RussianWord>();
            foreach (string word in rusToEng.Keys)
            {
                RussianWord w = new RussianWord();
                w.Word = word;
                w.Translations = rusToEng[word];
                russianWords.Add(w);
            }

            // синонимы — слова, у которых больше одного перевода
            synonyms = new List<SynonymInfo>();
            foreach (EnglishWord w in englishWords)
            {
                if (w.HasSynonyms)
                {
                    SynonymInfo s = new SynonymInfo();
                    s.Word = w.Word;
                    s.Direction = TranslationMode.EngToRus;
                    s.Synonyms = w.Translations;
                    synonyms.Add(s);
                }
            }
            foreach (RussianWord w in russianWords)
            {
                if (w.HasSynonyms)
                {
                    SynonymInfo s = new SynonymInfo();
                    s.Word = w.Word;
                    s.Direction = TranslationMode.RusToEng;
                    s.Synonyms = w.Translations;
                    synonyms.Add(s);
                }
            }

            // сколько слов в каждом направлении
            directions[0].WordsCount = engToRus.Count;
            directions[1].WordsCount = rusToEng.Count;
        }

        // ---------- перевод ----------

        // англ -> рус; если слова нет — пустой список
        public List<string> TranslateEngToRus(string word)
        {
            word = word.Trim();
            if (engToRus.ContainsKey(word))
                return new List<string>(engToRus[word]);
            return new List<string>();
        }

        // рус -> англ
        public List<string> TranslateRusToEng(string word)
        {
            word = word.Trim();
            if (rusToEng.ContainsKey(word))
                return new List<string>(rusToEng[word]);
            return new List<string>();
        }

        // перевод по выбранному режиму
        public List<string> Translate(string word, TranslationMode mode)
        {
            if (mode == TranslationMode.EngToRus)
                return TranslateEngToRus(word);
            else
                return TranslateRusToEng(word);
        }

        // ---------- запросы ----------

        // английские слова по алфавиту (A→Z) или наоборот (Z→A)
        public List<EnglishWord> GetEnglishWords(bool ascending)
        {
            List<EnglishWord> list = new List<EnglishWord>(englishWords);
            if (!ascending)
                list.Reverse();
            return list;
        }

        // русские слова по алфавиту (А→Я) или наоборот (Я→А)
        public List<RussianWord> GetRussianWords(bool ascending)
        {
            List<RussianWord> list = new List<RussianWord>(russianWords);
            if (!ascending)
                list.Reverse();
            return list;
        }

        // слова с несколькими переводами; direction = null — оба направления
        public List<SynonymInfo> GetSynonyms(TranslationMode? direction)
        {
            List<SynonymInfo> list = new List<SynonymInfo>();
            foreach (SynonymInfo s in synonyms)
            {
                if (direction == null || s.Direction == direction)
                    list.Add(s);
            }
            return list;
        }

        // направления перевода
        public List<TranslationDirection> GetDirections()
        {
            return directions;
        }

        // ---------- статистика ----------

        // цифры для плиток. Записей = сколько англ. слов (одна строка файла = одна запись)
        public DictionaryStats GetStats()
        {
            DictionaryStats stats = new DictionaryStats();
            stats.RecordsCount = engToRus.Count;
            stats.EngWordsCount = engToRus.Count;
            stats.RusWordsCount = rusToEng.Count;

            foreach (string word in engToRus.Keys)
                stats.TranslationPairsCount += engToRus[word].Count;

            foreach (SynonymInfo s in synonyms)
            {
                if (s.Direction == TranslationMode.EngToRus)
                    stats.EngWordsWithSynonymsCount++;
                else
                    stats.RusWordsWithSynonymsCount++;
            }

            return stats;
        }
    }
}
