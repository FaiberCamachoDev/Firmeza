using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.ViewModels.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin.Customers;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public CustomerCreateViewModel Input { get; set; } = new();

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var exists = await _db.Customers.AnyAsync(c => c.DocumentNumber == Input.DocumentNumber.Trim());
            if (exists)
            {
                ModelState.AddModelError(nameof(Input.DocumentNumber), "Ya existe un cliente con ese documento.");
                return Page();
            }

            var customer = new Customer
            {
                FirstName = Input.FirstName.Trim(),
                LastName = Input.LastName.Trim(),
                DocumentNumber = Input.DocumentNumber.Trim(),
                Phone = Input.Phone.Trim(),
                Email = Input.Email.Trim(),
                Address = Input.Address.Trim(),
                IsActive = true
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al registrar el cliente: {ex.Message}");
            return Page();
        }
    }
}
