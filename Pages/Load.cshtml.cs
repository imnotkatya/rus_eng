using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    // страница «Загрузка»: загрузка словаря из файла
    public class LoadModel : PageModel
    {
        private IDictionaryService dictionary;

        public LoadModel(IDictionaryService dict)
        {
            dictionary = dict;
        }

        // файл, который выбрал пользователь
        [BindProperty]
        public IFormFile? Upload { get; set; }

        public LoadResult Result { get; set; } = new LoadResult();
        public string? ErrorMessage { get; set; }
        public DictionaryStats Stats { get; set; } = new DictionaryStats();

        // просто открыли страницу — показываю, какой словарь сейчас загружен
        public void OnGet()
        {
            Result = dictionary.LastLoad;
            Stats = dictionary.GetStats();
        }

        // кнопка «Загрузить из Data/dictionary.txt»
        public void OnPostReload()
        {
            Result = dictionary.LoadFromFile();
            Stats = dictionary.GetStats();
        }

        // кнопка «Загрузить выбранный файл»
        public void OnPostUpload()
        {
            // сначала проверяю сам файл
            if (Upload == null || Upload.Length == 0)
            {
                ErrorMessage = "Выберите непустой файл словаря.";
            }
            else if (Path.GetExtension(Upload.FileName).ToLower() != ".txt")
            {
                ErrorMessage = "Файл словаря должен иметь расширение .txt.";
            }
            else if (Upload.Length > 1024 * 1024)
            {
                ErrorMessage = "Файл слишком большой (не более 1 МБ).";
            }

            if (ErrorMessage != null)
            {
                // файл плохой — оставляю старый словарь
                Result = dictionary.LastLoad;
                Stats = dictionary.GetStats();
                return;
            }

            // файл нормальный — загружаю
            using (Stream stream = Upload!.OpenReadStream())
            {
                Result = dictionary.LoadFromStream(stream, Upload.FileName);
            }
            Stats = dictionary.GetStats();
        }
    }
}
