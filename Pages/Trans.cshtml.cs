using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DictionaryWebApp.Pages;

public class TransModel : PageModel
{
    private readonly ILogger<TransModel> _logger;

    public TransModel(ILogger<TransModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}

