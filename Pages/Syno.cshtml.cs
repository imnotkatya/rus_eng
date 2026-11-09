using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    // страница «Синонимы»: слова, у которых несколько переводов
    public class SynoModel : PageModel
    {
        private IDictionaryService dictionary;

        public SynoModel(IDictionaryService dict)
        {
            dictionary = dict;
        }

        // направление из адреса; если не выбрано (null) — показываю оба
        [BindProperty(SupportsGet = true)]
        public TranslationMode? Mode { get; set; }

        public List<SynonymInfo> Words { get; set; } = new List<SynonymInfo>();

        public void OnGet()
        {
            Words = dictionary.GetSynonyms(Mode);
        }
    }
}
