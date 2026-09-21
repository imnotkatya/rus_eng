using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
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

        /// <summary>Сортировка: true — по возрастанию (А→Я), false — по убыванию (Я→А).</summary>
        [BindProperty(SupportsGet = true)]
        public bool Ascending { get; set; } = true;

        /// <summary>Слова для отображения (отсортированные).</summary>
        public List<KeyValuePair<string, List<string>>> Words { get; set; } = new();

        /// <summary>Заголовок первой колонки.</summary>
        public string FirstColumnTitle { get; set; } = "";

        /// <summary>Заголовок второй колонки.</summary>
        public string SecondColumnTitle { get; set; } = "";

        /// <summary>Общее количество слов.</summary>
        public int TotalCount { get; set; }

        public void OnGet()
        {
            // Получаем нужный словарь в зависимости от режима
            SortedDictionary<string, List<string>> source;

            if (Mode == TranslationMode.EngToRus)
            {
                source = _dictionary.GetAllEngToRus();
                FirstColumnTitle = "Английское слово";
                SecondColumnTitle = "Русские переводы";
            }
            else
            {
                source = _dictionary.GetAllRusToEng();
                FirstColumnTitle = "Русское слово";
                SecondColumnTitle = "Английские переводы";
            }

            // Преобразуем в список и сортируем по направлению
            Words = Ascending
                ? source.ToList()
                : source.Reverse().ToList();

            TotalCount = Words.Count;
        }
    }
}