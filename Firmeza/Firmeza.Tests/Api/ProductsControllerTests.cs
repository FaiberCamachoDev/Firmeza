using AutoMapper;
using Firmeza.Api.Controllers;
using Firmeza.Api.DTOs.Products;
using Firmeza.Api.Mappings;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests.Api;

public class ProductsControllerTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>())
            .CreateMapper();

        _controller = new ProductsController(_db, _mapper);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        _db.Products.AddRange(
            new Product { Name = "Cemento",   Price = 28m, Stock = 100, IsActive = true },
            new Product { Name = "Arena",     Price = 12m, Stock = 200, IsActive = true },
            new Product { Name = "Inactivo",  Price = 5m,  Stock = 10,  IsActive = false }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll(null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
        Assert.Equal(3, list.Count());
    }

    [Fact]
    public async Task GetAll_WithSearchFilter_ReturnsMatches()
    {
        _db.Products.AddRange(
            new Product { Name = "Cemento Portland",  Price = 28m, IsActive = true },
            new Product { Name = "Arena Fina",         Price = 12m, IsActive = true }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll("cemento", null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
        Assert.Single(list);
        Assert.Equal("Cemento Portland", list.First().Name);
    }

    [Fact]
    public async Task GetAll_WithActiveFilter_ReturnsOnlyActive()
    {
        _db.Products.AddRange(
            new Product { Name = "Activo",   Price = 10m, IsActive = true },
            new Product { Name = "Inactivo", Price = 5m,  IsActive = false }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll(null, null, active: true);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
        Assert.All(list, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsProduct()
    {
        var product = new Product { Name = "Varilla 1/2\"", Price = 38m, IsActive = true };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var result = await _controller.GetById(product.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ProductDto>(ok.Value);
        Assert.Equal("Varilla 1/2\"", dto.Name);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var result = await _controller.GetById(9999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreated()
    {
        var dto = new ProductCreateDto
        {
            Name     = "Ladrillo",
            Price    = 1.20m,
            Stock    = 5000,
            Category = "Ladrillos",
            Unit     = "und",
        };

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var productDto = Assert.IsType<ProductDto>(created.Value);
        Assert.Equal("Ladrillo", productDto.Name);
        Assert.Equal(1, await _db.Products.CountAsync());
    }

    [Fact]
    public async Task Update_ExistingProduct_UpdatesFields()
    {
        var product = new Product { Name = "Original", Price = 10m, IsActive = true };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var update = new ProductUpdateDto { Price = 15m, Name = "Actualizado" };
        var result = await _controller.Update(product.Id, update);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ProductDto>(ok.Value);
        Assert.Equal("Actualizado", dto.Name);
        Assert.Equal(15m, dto.Price);
    }

    [Fact]
    public async Task Delete_ProductWithNoSales_ReturnsNoContent()
    {
        var product = new Product { Name = "Sin ventas", Price = 5m, IsActive = true };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var result = await _controller.Delete(product.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await _db.Products.CountAsync());
    }

    [Fact]
    public async Task GetCategories_ReturnsDistinctCategories()
    {
        _db.Products.AddRange(
            new Product { Name = "P1", Price = 1m, Category = "Cementos", IsActive = true },
            new Product { Name = "P2", Price = 2m, Category = "Acero",    IsActive = true },
            new Product { Name = "P3", Price = 3m, Category = "Cementos", IsActive = true }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetCategories();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var cats = Assert.IsAssignableFrom<IEnumerable<string>>(ok.Value);
        Assert.Equal(2, cats.Count());
    }
}
