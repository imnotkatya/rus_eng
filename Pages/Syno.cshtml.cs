using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    public class SynoModel : PageModel
    {
        private readonly IDictionaryService _dictionary;

        public SynoModel(IDictionaryService dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>Слова с несколькими переводами.</summary>
        public List<KeyValuePair<string, List<string>>> Words { get; set; } = new();

        public void OnGet()
        {
            Words = _dictionary.GetWordsWithSynonyms();
        }
    }
}