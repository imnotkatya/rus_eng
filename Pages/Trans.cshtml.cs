using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
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

        /// <summary>Выбранный режим перевода.</summary>
        [BindProperty]
        public TranslationMode Mode { get; set; } = TranslationMode.EngToRus;

        /// <summary>Найденные переводы.</summary>
        public List<string>? Translations { get; set; }

        /// <summary>Флаг: была ли нажата кнопка «Перевести».</summary>
        public bool Searched { get; set; }

        /// <summary>Сообщение об ошибке (если слово пустое).</summary>
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Ничего не делаем при первом открытии страницы
        }

        public void OnPost()
        {
            Searched = true;

            if (string.IsNullOrWhiteSpace(Word))
            {
                ErrorMessage = "Введите слово для перевода.";
                return;
            }

            string trimmed = Word.Trim();

            Translations = Mode == TranslationMode.EngToRus
                ? _dictionary.TranslateEngToRus(trimmed)
                : _dictionary.TranslateRusToEng(trimmed);
        }
    }
}