using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.Web.Pages.Admin.Products;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ProductCreateViewModel Input { get; set; } = new();

    public string? StockError { get; set; }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        // Task 7: validar que Stock sea un entero válido
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
            var product = new Product
            {
                Name = Input.Name.Trim(),
                Description = Input.Description.Trim(),
                Category = Input.Category.Trim(),
                Unit = Input.Unit.Trim(),
                Price = Input.Price,
                Stock = stock,
                IsActive = true
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al guardar el producto: {ex.Message}");
            return Page();
        }
    }
}
