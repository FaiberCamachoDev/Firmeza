using System.Security.Claims;
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
    private readonly ILogger<SalesController> _logger;

    public SalesController(
        ApplicationDbContext db,
        IMapper mapper,
        IEmailService email,
        ILogger<SalesController> logger)
    {
        _db     = db;
        _mapper = mapper;
        _email  = email;
        _logger = logger;
    }

    /// <summary>Lista todas las ventas. [Admin]</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll()
    {
        var sales = await _db.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<SaleDto>>(sales));
    }

    /// <summary>Obtiene una venta por ID. [Admin siempre; Cliente solo si es su propia venta]</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDto>> GetById(int id)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound();

        // L8: Cliente puede ver solo sus propias ventas
        if (!User.IsInRole("Admin"))
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (sale.Customer?.Email != userEmail)
                return Forbid();
        }

        return Ok(_mapper.Map<SaleDto>(sale));
    }

    /// <summary>Registra una nueva venta. [Admin o Cliente]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Cliente")]
    public async Task<ActionResult<SaleDto>> Create([FromBody] SaleCreateDto dto)
    {
        // C2: Cliente solo puede crear ventas para su propio registro de cliente
        if (User.IsInRole("Cliente") && !User.IsInRole("Admin"))
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var ownCustomer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == userEmail && c.IsActive);

            if (ownCustomer is null)
                return BadRequest(new { message = "No se encontró tu registro de cliente activo." });

            if (dto.CustomerId != ownCustomer.Id)
                return Forbid();
        }

        // H6: el cliente debe existir y estar activo
        var customer = await _db.Customers.FindAsync(dto.CustomerId);
        if (customer is null || !customer.IsActive)
            return BadRequest(new { message = "Cliente no encontrado o inactivo." });

        // M3: agregar cantidades de items con el mismo ProductId
        var aggregated = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        var productIds = aggregated.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest(new { message = "Uno o más productos no existen o están inactivos." });

        var insufficientStock = aggregated
            .Where(item => products.First(p => p.Id == item.ProductId).Stock < item.Quantity)
            .Select(item => products.First(p => p.Id == item.ProductId).Name)
            .ToList();

        if (insufficientStock.Count > 0)
            return BadRequest(new { message = $"Stock insuficiente para: {string.Join(", ", insufficientStock)}." });

        var details = aggregated.Select(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            product.Stock -= item.Quantity;
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

        try
        {
            // H2: [ConcurrencyCheck] en Product.Stock detecta escrituras simultáneas;
            // si otro request decrementó el stock primero, EF lanza DbUpdateConcurrencyException
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "El stock cambió mientras procesabas la compra. Intenta nuevamente." });
        }

        sale.Customer = customer;
        foreach (var d in sale.Details)
            d.Product = products.First(p => p.Id == d.ProductId);

        // M4: email en background con log de error si falla
        _ = Task.Run(async () =>
        {
            try
            {
                await _email.SendPurchaseConfirmationAsync(
                    customer.Email, $"{customer.FirstName} {customer.LastName}",
                    sale.Id, sale.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de confirmación para venta {SaleId}", sale.Id);
            }
        });

        var result = await _db.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Details).ThenInclude(d => d.Product)
            .FirstAsync(s => s.Id == sale.Id);

        return CreatedAtAction(nameof(GetById), new { id = sale.Id },
            _mapper.Map<SaleDto>(result));
    }
}
