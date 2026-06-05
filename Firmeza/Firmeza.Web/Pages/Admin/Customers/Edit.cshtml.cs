using Firmeza.Web.Data;
using Firmeza.Web.ViewModels.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Customers;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public CustomerEditViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer is null) return NotFound();

            Input = new CustomerEditViewModel
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                DocumentNumber = customer.DocumentNumber,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                IsActive = customer.IsActive
            };
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var customer = await _db.Customers.FindAsync(Input.Id);
            if (customer is null) return NotFound();

            var duplicate = await _db.Customers.AnyAsync(c =>
                c.DocumentNumber == Input.DocumentNumber.Trim() && c.Id != Input.Id);
            if (duplicate)
            {
                ModelState.AddModelError(nameof(Input.DocumentNumber), "Ya existe otro cliente con ese documento.");
                return Page();
            }

            customer.FirstName = Input.FirstName.Trim();
            customer.LastName = Input.LastName.Trim();
            customer.DocumentNumber = Input.DocumentNumber.Trim();
            customer.Phone = Input.Phone.Trim();
            customer.Email = Input.Email.Trim();
            customer.Address = Input.Address.Trim();
            customer.IsActive = Input.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar: {ex.Message}");
            return Page();
        }
    }
}
