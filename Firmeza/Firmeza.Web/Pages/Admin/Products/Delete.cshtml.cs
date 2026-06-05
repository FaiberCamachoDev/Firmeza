using Firmeza.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.Web.Pages.Admin.Products;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DeleteModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int ProductStock { get; set; }
    private int _productId;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return NotFound();

            _productId = id;
            ProductName = product.Name;
            ProductCategory = product.Category;
            ProductStock = product.Stock;

            TempData["DeleteId"] = id;
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"No se pudo eliminar el producto: {ex.Message}");
            return Page();
        }
    }
}
