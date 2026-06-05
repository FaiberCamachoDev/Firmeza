using Firmeza.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.Web.Pages.Admin.Customers;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DeleteModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer is null) return NotFound();

            CustomerName = $"{customer.FirstName} {customer.LastName}";
            CustomerDocument = customer.DocumentNumber;
            CustomerEmail = customer.Email;
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
            var customer = await _db.Customers.FindAsync(id);
            if (customer is null) return NotFound();

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"No se pudo eliminar el cliente: {ex.Message}");
            return Page();
        }
    }
}
