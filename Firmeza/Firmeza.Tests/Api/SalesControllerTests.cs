using System.Security.Claims;
using AutoMapper;
using Firmeza.Api.Controllers;
using Firmeza.Api.DTOs.Sales;
using Firmeza.Api.Mappings;
using Firmeza.Api.Services;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firmeza.Tests.Api;

// Stub: evita llamadas SMTP reales en tests
file sealed class NoOpEmailService : IEmailService
{
    public Task SendAsync(string to, string toName, string subject, string htmlBody) => Task.CompletedTask;
    public Task SendWelcomeAsync(string to, string name) => Task.CompletedTask;
    public Task SendPurchaseConfirmationAsync(string to, string name, int saleId, decimal total) => Task.CompletedTask;
}

public class SalesControllerTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly SalesController _controller;

    public SalesControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _controller = new SalesController(_db, _mapper, new NoOpEmailService(), NullLogger<SalesController>.Instance)
        {
            // Simula un usuario Admin para que los checks de ownership/rol se salten en tests
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Role,  "Admin"),
                        new Claim(ClaimTypes.Email, "admin@test.com"),
                    ], "Test"))
                }
            }
        };
    }

    public void Dispose() => _db.Dispose();

    private async Task<(Customer customer, Product product)> SeedBasicDataAsync()
    {
        var customer = new Customer
        {
            FirstName = "Test", LastName = "Cliente",
            DocumentNumber = "T-001", Email = "test@test.com", IsActive = true,
        };
        var product = new Product
        {
            Name = "Cemento", Price = 25m, Stock = 100, IsActive = true,
        };
        _db.Customers.Add(customer);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return (customer, product);
    }

    [Fact]
    public async Task GetAll_ReturnsSalesOrderedByDateDesc()
    {
        var (customer, product) = await SeedBasicDataAsync();

        _db.Sales.AddRange(
            new Sale { CustomerId = customer.Id, Total = 100m, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Sale { CustomerId = customer.Id, Total = 200m, CreatedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<SaleDto>>(ok.Value).ToList();
        Assert.Equal(2, list.Count);
        Assert.True(list[0].CreatedAt >= list[1].CreatedAt);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsSale()
    {
        var (customer, product) = await SeedBasicDataAsync();
        var sale = new Sale { CustomerId = customer.Id, Total = 50m };
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        var result = await _controller.GetById(sale.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<SaleDto>(ok.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var result = await _controller.GetById(9999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidSale_ReturnsCreatedWithCorrectTotal()
    {
        var (customer, product) = await SeedBasicDataAsync();

        var dto = new SaleCreateDto
        {
            CustomerId = customer.Id,
            Items = [new SaleDetailCreateDto { ProductId = product.Id, Quantity = 4 }],
        };

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var saleDto = Assert.IsType<SaleDto>(created.Value);
        Assert.Equal(100m, saleDto.Total);
    }

    [Fact]
    public async Task Create_DecrementsProductStock()
    {
        var (customer, product) = await SeedBasicDataAsync();
        var initialStock = product.Stock;

        var dto = new SaleCreateDto
        {
            CustomerId = customer.Id,
            Items = [new SaleDetailCreateDto { ProductId = product.Id, Quantity = 10 }],
        };

        await _controller.Create(dto);

        var updated = await _db.Products.FindAsync(product.Id);
        Assert.Equal(initialStock - 10, updated!.Stock);
    }

    [Fact]
    public async Task Create_InsufficientStock_ReturnsBadRequest()
    {
        var (customer, product) = await SeedBasicDataAsync();

        var dto = new SaleCreateDto
        {
            CustomerId = customer.Id,
            Items = [new SaleDetailCreateDto { ProductId = product.Id, Quantity = 999 }],
        };

        var result = await _controller.Create(dto);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_InvalidCustomer_ReturnsBadRequest()
    {
        var (_, product) = await SeedBasicDataAsync();

        var dto = new SaleCreateDto
        {
            CustomerId = 9999,
            Items = [new SaleDetailCreateDto { ProductId = product.Id, Quantity = 1 }],
        };

        var result = await _controller.Create(dto);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_InactiveProduct_ReturnsBadRequest()
    {
        var (customer, _) = await SeedBasicDataAsync();
        var inactiveProduct = new Product { Name = "Inactivo", Price = 10m, Stock = 50, IsActive = false };
        _db.Products.Add(inactiveProduct);
        await _db.SaveChangesAsync();

        var dto = new SaleCreateDto
        {
            CustomerId = customer.Id,
            Items = [new SaleDetailCreateDto { ProductId = inactiveProduct.Id, Quantity = 1 }],
        };

        var result = await _controller.Create(dto);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_MultipleItems_TotalIsSumOfSubtotals()
    {
        var (customer, product) = await SeedBasicDataAsync();
        var product2 = new Product { Name = "Varilla", Price = 10m, Stock = 50, IsActive = true };
        _db.Products.Add(product2);
        await _db.SaveChangesAsync();

        var dto = new SaleCreateDto
        {
            CustomerId = customer.Id,
            Items =
            [
                new SaleDetailCreateDto { ProductId = product.Id,  Quantity = 2 },
                new SaleDetailCreateDto { ProductId = product2.Id, Quantity = 3 },
            ],
        };

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var saleDto = Assert.IsType<SaleDto>(created.Value);
        Assert.Equal(2 * 25m + 3 * 10m, saleDto.Total);
    }
}
