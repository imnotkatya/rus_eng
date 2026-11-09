using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    // страница «Перевод»
    public class TransModel : PageModel
    {
        private IDictionaryService dictionary;

        // словарь приходит сам (его создали в Program.cs)
        public TransModel(IDictionaryService dict)
        {
            dictionary = dict;
        }

        // слово из поля ввода
        [BindProperty]
        public string? Word { get; set; }

        // выбранный режим
        [BindProperty]
        public TranslationMode Mode { get; set; } = TranslationMode.EngToRus;

        public List<TranslationDirection> Directions { get; set; } = new List<TranslationDirection>();
        public List<string>? Translations { get; set; }   // null — ещё не искали
        public string? ErrorMessage { get; set; }         // текст ошибки ввода

        // просто открыли страницу
        public void OnGet()
        {
            Directions = dictionary.GetDirections();
        }

        // нажали «Перевести»
        public void OnPost()
        {
            Directions = dictionary.GetDirections();

            // сначала проверяю, правильно ли введено слово
            ErrorMessage = WordValidator.Validate(Word, Mode);
            if (ErrorMessage == null)
            {
                Word = Word!.Trim();
                Translations = dictionary.Translate(Word, Mode);
            }
        }
    }
}
