using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    /// <summary>
    /// Пункт меню «Перевод»: выбор режима работы и вывод перевода
    /// заданного английского или русского слова.
    /// </summary>
    public class TransModel : PageModel
    {
        private readonly IDictionaryService _dictionary;

        public TransModel(IDictionaryService dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>Введённое слово.</summary>
        [BindProperty]
        public string Word { get; set; } = "";

        /// <summary>Выбранный режим работы (направление перевода).</summary>
        [BindProperty(SupportsGet = true)]
        public TranslationMode Mode { get; set; } = TranslationMode.EngToRus;

        /// <summary>Список направлений перевода для выбора режима.</summary>
        public List<TranslationDirection> Directions { get; set; } = new();

        /// <summary>Список найденных переводов.</summary>
        public List<string>? Translations { get; set; }

        /// <summary>Флаг: была ли нажата кнопка «Перевести».</summary>
        public bool Searched { get; set; }

        /// <summary>Сообщение об ошибке ввода.</summary>
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            Directions = _dictionary.GetDirections();
        }

        public void OnPost()
        {
            Directions = _dictionary.GetDirections();
            Searched = true;

            // Контроль правильности ввода
            ErrorMessage = WordValidator.Validate(Word, Mode);
            if (ErrorMessage != null)
                return;

            Word = Word.Trim();
            Translations = _dictionary.Translate(Word, Mode);
        }
    }
}
