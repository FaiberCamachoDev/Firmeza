using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Products;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Product> Products { get; set; } = [];
    public string? SearchTerm { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? search)
    {
        try
        {
            SearchTerm = search;
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search.ToLower()) ||
                    p.Category.ToLower().Contains(search.ToLower()));

            Products = await query.OrderBy(p => p.Name).ToListAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar productos: {ex.Message}";
        }
    }
}
