using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.Services;
using Firmeza.Web.ViewModels.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Sales;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly PdfReceiptService _pdfReceipt;

    public CreateModel(ApplicationDbContext db, PdfReceiptService pdfReceipt)
    {
        _db = db;
        _pdfReceipt = pdfReceipt;
    }

    [BindProperty]
    public SaleCreateViewModel Input { get; set; } = new();

    public SelectList Customers { get; private set; } = null!;
    public SelectList Products { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        await LoadSelectsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Items is null || Input.Items.Count == 0)
            ModelState.AddModelError(string.Empty, "Agrega al menos un producto.");

        if (!ModelState.IsValid)
        {
            await LoadSelectsAsync();
            return Page();
        }

        var products = await _db.Products
            .Where(p => Input.Items!.Select(i => i.ProductId).Contains(p.Id))
            .ToListAsync();

        var details = Input.Items!.Select(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return new SaleDetail
            {
                ProductId = item.ProductId,
                Quantity  = item.Quantity,
                UnitPrice = product.Price,
            };
        }).ToList();

        var sale = new Sale
        {
            CustomerId = Input.CustomerId,
            Notes      = Input.Notes.Trim(),
            Total      = details.Sum(d => d.Quantity * d.UnitPrice),
            Details    = details,
        };

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        // Cargar navegaciones para el PDF
        sale.Customer = await _db.Customers.FindAsync(sale.CustomerId) ?? sale.Customer;
        foreach (var d in sale.Details)
            d.Product = products.First(p => p.Id == d.ProductId);

        _pdfReceipt.Generate(sale);

        return RedirectToPage("Details", new { id = sale.Id });
    }

    private async Task LoadSelectsAsync()
    {
        var customers = await _db.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.LastName)
            .Select(c => new { c.Id, Name = $"{c.FirstName} {c.LastName} — {c.DocumentNumber}" })
            .ToListAsync();

        var prods = await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, Name = $"{p.Name} (${p.Price:N2})" })
            .ToListAsync();

        Customers = new SelectList(customers, "Id", "Name");
        Products  = new SelectList(prods, "Id", "Name");
    }
}
