using Firmeza.Web.Data;
using Firmeza.Web.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Products;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ProductEditViewModel Input { get; set; } = new();

    public string? StockError { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return NotFound();

            Input = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Unit = product.Unit,
                Price = product.Price,
                StockInput = product.Stock.ToString(),
                IsActive = product.IsActive
            };
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al cargar el producto: {ex.Message}");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Task 7: validación de Stock como entero
        int stock;
        if (!int.TryParse(Input.StockInput, out stock) || stock < 0)
        {
            StockError = "El stock debe ser un número entero mayor o igual a 0.";
            ModelState.AddModelError(nameof(Input.StockInput), StockError);
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var product = await _db.Products.FindAsync(Input.Id);
            if (product is null) return NotFound();

            product.Name = Input.Name.Trim();
            product.Description = Input.Description.Trim();
            product.Category = Input.Category.Trim();
            product.Unit = Input.Unit.Trim();
            product.Price = Input.Price;
            product.Stock = stock;
            product.IsActive = Input.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty, "El registro fue modificado por otro usuario. Recargue e intente nuevamente.");
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar: {ex.Message}");
            return Page();
        }
    }
}
