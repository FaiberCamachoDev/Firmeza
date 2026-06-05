using Firmeza.Web.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DashboardModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalSales { get; set; }
    public int ActiveProducts { get; set; }
    public int ActiveCustomers { get; set; }
    public int SalesThisMonth { get; set; }

    public async Task OnGetAsync()
    {
        var now = DateTime.UtcNow;

        TotalProducts = await _db.Products.CountAsync();
        TotalCustomers = await _db.Customers.CountAsync();
        TotalSales = await _db.Sales.CountAsync();
        ActiveProducts = await _db.Products.CountAsync(p => p.IsActive);
        ActiveCustomers = await _db.Customers.CountAsync(c => c.IsActive);
        SalesThisMonth = await _db.Sales.CountAsync(s =>
            s.CreatedAt.Month == now.Month && s.CreatedAt.Year == now.Year);
    }
}
