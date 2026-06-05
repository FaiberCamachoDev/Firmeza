using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Customers;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Customer> Customers { get; set; } = [];
    public string? SearchTerm { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? search)
    {
        try
        {
            SearchTerm = search;
            var query = _db.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c =>
                    (c.FirstName + " " + c.LastName).ToLower().Contains(search.ToLower()) ||
                    c.DocumentNumber.Contains(search));

            Customers = await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar clientes: {ex.Message}";
        }
    }
}
