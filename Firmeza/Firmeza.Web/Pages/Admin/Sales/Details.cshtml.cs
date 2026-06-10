using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Sales;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly PdfReceiptService _pdfReceipt;

    public DetailsModel(ApplicationDbContext db, PdfReceiptService pdfReceipt)
    {
        _db = db;
        _pdfReceipt = pdfReceipt;
    }

    public Sale Sale { get; private set; } = null!;
    public string? ReceiptPath { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound();

        Sale = sale;

        if (_pdfReceipt.Exists(sale.Id, sale.CreatedAt))
            ReceiptPath = _pdfReceipt.GetRelativePath(sale.Id, sale.CreatedAt);

        return Page();
    }

    // Regenerate the receipt on demand.
    public async Task<IActionResult> OnPostRegenerateAsync(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound();

        _pdfReceipt.Generate(sale);

        return RedirectToPage(new { id });
    }
}
