using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Sales;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ExcelExportService _excelExport;
    private readonly PdfReceiptService _pdfReceipt;

    public IndexModel(ApplicationDbContext db, ExcelExportService excelExport, PdfReceiptService pdfReceipt)
    {
        _db = db;
        _excelExport = excelExport;
        _pdfReceipt = pdfReceipt;
    }

    public List<Sale> Sales { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Sales = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Details)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetExportExcelAsync()
    {
        var bytes = await _excelExport.ExportSalesAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ventas-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    public string GetReceiptPath(Sale sale) =>
        _pdfReceipt.Exists(sale.Id, sale.CreatedAt)
            ? _pdfReceipt.GetRelativePath(sale.Id, sale.CreatedAt)
            : string.Empty;
}
