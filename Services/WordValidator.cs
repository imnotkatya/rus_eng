using DictionaryWebApp.Models;

namespace DictionaryWebApp.Services
{
    // Проверка правильности вводимых слов
    public static class WordValidator
    {
        // Максимальная длина слова
        public const int MaxLength = 50;

        // Английская буква?
        private static bool IsEnglishLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        // Русская буква?
        private static bool IsRussianLetter(char c)
        {
            return (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
        }

        // Английское слово: латинские буквы, внутри можно пробел, дефис и апостроф
        public static bool IsEnglish(string word)
        {
            if (word.Length == 0) return false;

            // Первый и последний символ должны быть буквами
            if (!IsEnglishLetter(word[0]) || !IsEnglishLetter(word[word.Length - 1])) return false;

            foreach (char c in word)
            {
                if (!IsEnglishLetter(c) && c != ' ' && c != '-' && c != '\'')
                    return false;
            }
            return true;
        }

        // Русское слово: русские буквы, внутри можно пробел и дефис
        public static bool IsRussian(string word)
        {
            if (word.Length == 0) return false;

            // Первый и последний символ должны быть буквами
            if (!IsRussianLetter(word[0]) || !IsRussianLetter(word[word.Length - 1])) return false;

            foreach (char c in word)
            {
                if (!IsRussianLetter(c) && c != ' ' && c != '-')
                    return false;
            }
            return true;
        }

        // Проверить слово перед переводом.
        // Возвращает текст ошибки или null, если всё правильно.
        public static string? Validate(string? word, TranslationMode mode)
        {
            if (string.IsNullOrWhiteSpace(word))
                return "Введите слово для перевода.";

            word = word.Trim();

            if (word.Length > MaxLength)
                return $"Слово слишком длинное (не более {MaxLength} символов).";

            if (mode == TranslationMode.EngToRus && !IsEnglish(word))
            {
                if (IsRussian(word))
                    return "Выбран англо-русский режим, а введено русское слово. Смените режим или введите английское слово.";
                return "Английское слово должно состоять только из латинских букв.";
            }

            if (mode == TranslationMode.RusToEng && !IsRussian(word))
            {
                if (IsEnglish(word))
                    return "Выбран русско-английский режим, а введено английское слово. Смените режим или введите русское слово.";
                return "Русское слово должно состоять только из русских букв.";
            }

            return null;
        }
    }
}
