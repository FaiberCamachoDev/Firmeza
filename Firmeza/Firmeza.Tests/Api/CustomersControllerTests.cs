using AutoMapper;
using Firmeza.Api.Controllers;
using Firmeza.Api.DTOs.Customers;
using Firmeza.Api.Mappings;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Tests.Api;

public class CustomersControllerTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
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

        _controller = new CustomersController(_db, _mapper);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetAll_ReturnsAllCustomers()
    {
        _db.Customers.AddRange(
            new Customer { FirstName = "Juan",  LastName = "Pérez",  DocumentNumber = "001", IsActive = true },
            new Customer { FirstName = "María", LastName = "López",  DocumentNumber = "002", IsActive = true }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll(null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(ok.Value);
        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task GetAll_WithSearchFilter_ReturnsMatches()
    {
        _db.Customers.AddRange(
            new Customer { FirstName = "Carlos", LastName = "García",  DocumentNumber = "111", IsActive = true },
            new Customer { FirstName = "Ana",    LastName = "Martínez", DocumentNumber = "222", IsActive = true }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll("carlos", null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(ok.Value);
        Assert.Single(list);
        Assert.Equal("Carlos", list.First().FirstName);
    }

    [Fact]
    public async Task GetAll_WithActiveFilter_ReturnsOnlyActive()
    {
        _db.Customers.AddRange(
            new Customer { FirstName = "Activo",   LastName = "X", DocumentNumber = "A1", IsActive = true },
            new Customer { FirstName = "Inactivo", LastName = "X", DocumentNumber = "A2", IsActive = false }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll(null, active: true);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(ok.Value);
        Assert.All(list, c => Assert.True(c.IsActive));
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCustomer()
    {
        var customer = new Customer { FirstName = "Luis", LastName = "Torres", DocumentNumber = "999", IsActive = true };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        var result = await _controller.GetById(customer.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CustomerDto>(ok.Value);
        Assert.Equal("Luis", dto.FirstName);
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
        var dto = new CustomerCreateDto
        {
            FirstName      = "Pedro",
            LastName       = "Ruiz",
            DocumentNumber = "DOC-001",
            Email          = "pedro@test.com",
        };

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var customerDto = Assert.IsType<CustomerDto>(created.Value);
        Assert.Equal("Pedro", customerDto.FirstName);
        Assert.Equal(1, await _db.Customers.CountAsync());
    }

    [Fact]
    public async Task Create_DuplicateDocument_ReturnsConflict()
    {
        _db.Customers.Add(new Customer { FirstName = "X", LastName = "Y", DocumentNumber = "DUP-001", IsActive = true });
        await _db.SaveChangesAsync();

        var dto = new CustomerCreateDto
        {
            FirstName      = "Otro",
            LastName       = "Nombre",
            DocumentNumber = "DUP-001",
        };

        var result = await _controller.Create(dto);
        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ExistingCustomer_UpdatesFields()
    {
        var customer = new Customer { FirstName = "Original", LastName = "Apellido", DocumentNumber = "UPD-001", IsActive = true };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        var update = new CustomerUpdateDto { FirstName = "Actualizado", Phone = "123456789" };
        var result = await _controller.Update(customer.Id, update);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CustomerDto>(ok.Value);
        Assert.Equal("Actualizado", dto.FirstName);
        Assert.Equal("123456789", dto.Phone);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var result = await _controller.Update(9999, new CustomerUpdateDto { FirstName = "X" });
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ExistingCustomer_SoftDeletesAndReturnsNoContent()
    {
        var customer = new Customer { FirstName = "Borrar", LastName = "Me", DocumentNumber = "DEL-001", IsActive = true };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        var result = await _controller.Delete(customer.Id);

        Assert.IsType<NoContentResult>(result);
        var updated = await _db.Customers.FindAsync(customer.Id);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var result = await _controller.Delete(9999);
        Assert.IsType<NotFoundResult>(result);
    }
}
