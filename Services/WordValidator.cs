using System.Text.RegularExpressions;
using DictionaryWebApp.Models;

namespace DictionaryWebApp.Services
{
    /// <summary>
    /// Контроль правильности вводимых слов.
    /// </summary>
    public static class WordValidator
    {
        /// <summary>Максимальная длина слова.</summary>
        public const int MaxLength = 50;

        // Английское слово: латинские буквы, внутри допускаются дефис, апостроф и пробел
        private static readonly Regex EnglishRegex =
            new(@"^[A-Za-z]+([ '\-][A-Za-z]+)*$", RegexOptions.Compiled);

        // Русское слово: кириллица, внутри допускаются дефис и пробел
        private static readonly Regex RussianRegex =
            new(@"^[А-Яа-яЁё]+([ \-][А-Яа-яЁё]+)*$", RegexOptions.Compiled);

        /// <summary>Является ли строка корректным английским словом.</summary>
        public static bool IsEnglish(string word) => EnglishRegex.IsMatch(word);

        /// <summary>Является ли строка корректным русским словом.</summary>
        public static bool IsRussian(string word) => RussianRegex.IsMatch(word);

        /// <summary>
        /// Проверить слово, введённое для перевода в заданном режиме.
        /// Возвращает текст ошибки или null, если слово введено правильно.
        /// </summary>
        public static string? Validate(string? word, TranslationMode mode)
        {
            if (string.IsNullOrWhiteSpace(word))
                return "Введите слово для перевода.";

            word = word.Trim();

            if (word.Length > MaxLength)
                return $"Слово слишком длинное (не более {MaxLength} символов).";

            if (mode == TranslationMode.EngToRus && !IsEnglish(word))
                return IsRussian(word)
                    ? "Выбран англо-русский режим, а введено русское слово. Смените режим или введите английское слово."
                    : "Английское слово должно состоять только из латинских букв.";

            if (mode == TranslationMode.RusToEng && !IsRussian(word))
                return IsEnglish(word)
                    ? "Выбран русско-английский режим, а введено английское слово. Смените режим или введите русское слово."
                    : "Русское слово должно состоять только из русских букв.";

            return null;
        }
    }
}
