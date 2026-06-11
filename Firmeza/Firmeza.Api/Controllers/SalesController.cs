using AutoMapper;
using Firmeza.Api.DTOs.Sales;
using Firmeza.Api.Services;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IEmailService _email;

    public SalesController(ApplicationDbContext db, IMapper mapper, IEmailService email)
    {
        _db = db;
        _mapper = mapper;
        _email = email;
    }

    /// <summary>Lista todas las ventas. [Admin]</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll()
    {
        var sales = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<SaleDto>>(sales));
    }

    /// <summary>Obtiene una venta por ID. [Admin o propietario]</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDto>> GetById(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound();
        return Ok(_mapper.Map<SaleDto>(sale));
    }

    /// <summary>Registra una nueva venta. [Admin o Cliente]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Cliente")]
    public async Task<ActionResult<SaleDto>> Create([FromBody] SaleCreateDto dto)
    {
        var customer = await _db.Customers.FindAsync(dto.CustomerId);
        if (customer is null)
            return BadRequest(new { message = "Cliente no encontrado." });

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest(new { message = "Uno o más productos no existen o están inactivos." });

        var details = dto.Items.Select(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return new SaleDetail
            {
                ProductId = item.ProductId,
                Quantity  = item.Quantity,
                UnitPrice = product.Price,
            };
        }).ToList();

        var sale = new Sale
        {
            CustomerId = dto.CustomerId,
            Notes      = dto.Notes,
            Total      = details.Sum(d => d.Quantity * d.UnitPrice),
            Details    = details,
        };

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        // Cargar navegaciones para el mapeo y el email
        sale.Customer = customer;
        foreach (var d in sale.Details)
            d.Product = products.First(p => p.Id == d.ProductId);

        _ = _email.SendPurchaseConfirmationAsync(
            customer.Email, $"{customer.FirstName} {customer.LastName}",
            sale.Id, sale.Total);

        var result = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .FirstAsync(s => s.Id == sale.Id);

        return CreatedAtAction(nameof(GetById), new { id = sale.Id },
            _mapper.Map<SaleDto>(result));
    }
}
