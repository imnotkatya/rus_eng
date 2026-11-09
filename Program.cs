using DictionaryWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Подключаем страницы Razor
builder.Services.AddRazorPages();

// Создаём словарь один раз на всю программу
string path = Path.Combine(builder.Environment.ContentRootPath, "Data", "dictionary.txt");
builder.Services.AddSingleton<IDictionaryService>(new DictionaryService(path));

var app = builder.Build();

app.UseStaticFiles();   // отдаём стили и скрипты из wwwroot
app.MapRazorPages();    // подключаем страницы из папки Pages

app.Run();