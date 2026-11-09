using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    // страница «Словарь»: все слова по алфавиту
    public class DictModel : PageModel
    {
        private IDictionaryService dictionary;

        public DictModel(IDictionaryService dict)
        {
            dictionary = dict;
        }

        // эти два значения приходят из адреса, например /Dict?Mode=RusToEng&Ascending=false
        [BindProperty(SupportsGet = true)]
        public TranslationMode Mode { get; set; } = TranslationMode.EngToRus;

        [BindProperty(SupportsGet = true)]
        public bool Ascending { get; set; } = true;

        // строки таблицы: слово и его переводы (номер i в обоих списках — одна строка)
        public List<string> Words { get; set; } = new List<string>();
        public List<string> Translations { get; set; } = new List<string>();

        public string FirstColumnTitle { get; set; } = "";
        public string SecondColumnTitle { get; set; } = "";
        public DictionaryStats Stats { get; set; } = new DictionaryStats();
        public IEnumerable<TranslationDirection> Directions { get; internal set; }

        public void OnGet()
        {
            Stats = dictionary.GetStats();

            if (Mode == TranslationMode.EngToRus)
            {
                // английские слова с русскими переводами
                foreach (EnglishWord w in dictionary.GetEnglishWords(Ascending))
                {
                    Words.Add(w.Word);
                    Translations.Add(string.Join(", ", w.Translations));
                }
                FirstColumnTitle = "Английское слово";
                SecondColumnTitle = "Русские переводы";
            }
            else
            {
                // русские слова с английскими переводами
                foreach (RussianWord w in dictionary.GetRussianWords(Ascending))
                {
                    Words.Add(w.Word);
                    Translations.Add(string.Join(", ", w.Translations));
                }
                FirstColumnTitle = "Русское слово";
                SecondColumnTitle = "Английские переводы";
            }
        }
    }
}
