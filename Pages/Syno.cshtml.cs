using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    /// <summary>
    /// Пункт меню «Синонимы»: слова, имеющие несколько синонимичных переводов.
    /// </summary>
    public class SynoModel : PageModel
    {
        private readonly IDictionaryService _dictionary;

        public SynoModel(IDictionaryService dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>Направление перевода; null — оба направления.</summary>
        [BindProperty(SupportsGet = true)]
        public TranslationMode? Mode { get; set; }

        /// <summary>Список направлений перевода.</summary>
        public List<TranslationDirection> Directions { get; set; } = new();

        /// <summary>Слова с несколькими переводами.</summary>
        public List<SynonymInfo> Words { get; set; } = new();

        public void OnGet()
        {
            Directions = _dictionary.GetDirections();
            Words = _dictionary.GetSynonyms(Mode);
        }
    }
}
