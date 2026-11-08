using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    /// <summary>
    /// Пункт меню «Словарь»: список английских (или русских) слов
    /// по алфавиту с переводами, сортировка и общее количество записей.
    /// </summary>
    public class DictModel : PageModel
    {
        private readonly IDictionaryService _dictionary;

        public DictModel(IDictionaryService dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>Выбранное направление перевода.</summary>
        [BindProperty(SupportsGet = true)]
        public TranslationMode Mode { get; set; } = TranslationMode.EngToRus;

        /// <summary>Сортировка: true — по алфавиту (А→Я), false — в обратном порядке (Я→А).</summary>
        [BindProperty(SupportsGet = true)]
        public bool Ascending { get; set; } = true;

        /// <summary>Список направлений перевода.</summary>
        public List<TranslationDirection> Directions { get; set; } = new();

        /// <summary>Строки таблицы: слово и его переводы.</summary>
        public List<KeyValuePair<string, List<string>>> Words { get; set; } = new();

        /// <summary>Заголовок первой колонки.</summary>
        public string FirstColumnTitle { get; set; } = "";

        /// <summary>Заголовок второй колонки.</summary>
        public string SecondColumnTitle { get; set; } = "";

        /// <summary>Статистика словаря (общее количество записей и др.).</summary>
        public DictionaryStats Stats { get; set; } = new();

        public void OnGet()
        {
            Directions = _dictionary.GetDirections();
            Stats = _dictionary.GetStats();

            if (Mode == TranslationMode.EngToRus)
            {
                // Запрос: список английских слов по алфавиту с русскими переводами
                Words = _dictionary.GetEnglishWords(Ascending)
                    .Select(w => new KeyValuePair<string, List<string>>(w.Word, w.Translations))
                    .ToList();
                FirstColumnTitle = "Английское слово";
                SecondColumnTitle = "Русские переводы";
            }
            else
            {
                // Запрос: список русских слов по алфавиту с английскими переводами
                Words = _dictionary.GetRussianWords(Ascending)
                    .Select(w => new KeyValuePair<string, List<string>>(w.Word, w.Translations))
                    .ToList();
                FirstColumnTitle = "Русское слово";
                SecondColumnTitle = "Английские переводы";
            }
        }
    }
}
