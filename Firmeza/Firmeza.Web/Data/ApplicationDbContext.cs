using Firmeza.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleDetail> SaleDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Name);
            e.HasIndex(p => p.Category);
        });

        builder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.DocumentNumber).IsUnique();
            e.HasIndex(c => c.Email);
        });

        builder.Entity<Sale>(e =>
        {
            e.HasOne(s => s.Customer)
             .WithMany(c => c.Sales)
             .HasForeignKey(s => s.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleDetail>(e =>
        {
            e.HasOne(d => d.Sale)
             .WithMany(s => s.Details)
             .HasForeignKey(d => d.SaleId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Product)
             .WithMany(p => p.SaleDetails)
             .HasForeignKey(d => d.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            // Subtotal es calculado — no se persiste en DB
            e.Ignore(d => d.Subtotal);
        });
    }
}
