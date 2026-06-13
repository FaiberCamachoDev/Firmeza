using Firmeza.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.Web.Pages.Admin.Import;

public class IndexModel : PageModel
{
    private readonly ExcelImportService _importService;

    public IndexModel(ExcelImportService importService)
    {
        _importService = importService;
    }

    public ImportResult? Result { get; private set; }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Selecciona un archivo .xlsx.");
            return Page();
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "El archivo debe ser formato .xlsx.");
            return Page();
        }

        using var stream = file.OpenReadStream();
        Result = await _importService.ImportAsync(stream);

        return Page();
    }
}
