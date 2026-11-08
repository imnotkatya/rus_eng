namespace DictionaryWebApp.Models
{
    /// <summary>
    /// Результат загрузки «базы данных» словаря из файла.
    /// </summary>
    public class LoadResult
    {
        /// <summary>Успешно ли загружен словарь.</summary>
        public bool Success { get; set; }

        /// <summary>Имя загруженного файла.</summary>
        public string FileName { get; set; } = "";

        /// <summary>Количество загруженных записей.</summary>
        public int LoadedRecords { get; set; }

        /// <summary>Количество пропущенных (ошибочных) строк.</summary>
        public int SkippedLines { get; set; }

        /// <summary>Описание ошибок в файле.</summary>
        public List<string> Errors { get; } = new();

        /// <summary>Время загрузки.</summary>
        public DateTime LoadedAt { get; set; } = DateTime.Now;
    }
}
