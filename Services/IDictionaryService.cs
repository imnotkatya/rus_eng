using DictionaryWebApp.Models;

namespace DictionaryWebApp.Services
{
    // список того, что умеет словарь (сами методы написаны в DictionaryService)
    public interface IDictionaryService
    {
        // загрузка
        LoadResult LoadFromFile();                                   // из Data/dictionary.txt
        LoadResult LoadFromStream(Stream stream, string fileName);   // из выбранного файла
        LoadResult LastLoad { get; }                                 // результат последней загрузки

        // перевод
        List<string> TranslateEngToRus(string word);
        List<string> TranslateRusToEng(string word);
        List<string> Translate(string word, TranslationMode mode);

        // списки
        List<EnglishWord> GetEnglishWords(bool ascending);
        List<RussianWord> GetRussianWords(bool ascending);
        List<SynonymInfo> GetSynonyms(TranslationMode? direction);   // null — оба направления
        List<TranslationDirection> GetDirections();

        // статистика
        DictionaryStats GetStats();
    }
}
