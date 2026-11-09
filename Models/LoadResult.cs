namespace DictionaryWebApp.Models
{
    // что получилось при загрузке файла
    public class LoadResult
    {
        public bool Success { get; set; }                  // загрузился ли словарь
        public string FileName { get; set; } = "";         // имя файла
        public int LoadedRecords { get; set; }             // сколько записей загрузилось
        public int SkippedLines { get; set; }              // сколько строк с ошибками
        public List<string> Errors { get; set; } = new List<string>();   // тексты ошибок
        public DateTime LoadedAt { get; set; } = DateTime.Now;           // когда загрузили
    }
}
