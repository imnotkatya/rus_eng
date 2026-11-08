using DictionaryWebApp.Models;
using DictionaryWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages
{
    /// <summary>
    /// Пункт меню «Загрузка»: загрузка «базы данных» словаря из файла.
    /// </summary>
    public class LoadModel : PageModel
    {
        /// <summary>Максимальный размер загружаемого файла — 1 МБ.</summary>
        private const long MaxFileSize = 1024 * 1024;

        private readonly IDictionaryService _dictionary;

        public LoadModel(IDictionaryService dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>Файл, выбранный пользователем.</summary>
        [BindProperty]
        public IFormFile? Upload { get; set; }

        /// <summary>Результат загрузки (последней или только что выполненной).</summary>
        public LoadResult Result { get; set; } = new();

        /// <summary>Была ли загрузка выполнена на этом запросе.</summary>
        public bool JustLoaded { get; set; }

        /// <summary>Сообщение об ошибке выбора файла.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Статистика словаря.</summary>
        public DictionaryStats Stats { get; set; } = new();

        public void OnGet()
        {
            Result = _dictionary.LastLoad;
            Stats = _dictionary.GetStats();
        }

        /// <summary>Перезагрузить словарь из файла по умолчанию Data/dictionary.txt.</summary>
        public void OnPostReload()
        {
            Result = _dictionary.LoadFromFile();
            JustLoaded = true;
            Stats = _dictionary.GetStats();
        }

        /// <summary>Загрузить словарь из файла, выбранного пользователем.</summary>
        public void OnPostUpload()
        {
            if (Upload == null || Upload.Length == 0)
                ErrorMessage = "Выберите непустой файл словаря.";
            else if (!Path.GetExtension(Upload.FileName).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                ErrorMessage = "Файл словаря должен иметь расширение .txt.";
            else if (Upload.Length > MaxFileSize)
                ErrorMessage = "Файл слишком большой (не более 1 МБ).";

            if (ErrorMessage != null)
            {
                Result = _dictionary.LastLoad;
                Stats = _dictionary.GetStats();
                return;
            }

            using var stream = Upload!.OpenReadStream();
            Result = _dictionary.LoadFromStream(stream, Path.GetFileName(Upload.FileName));
            JustLoaded = true;
            Stats = _dictionary.GetStats();
        }
    }
}
