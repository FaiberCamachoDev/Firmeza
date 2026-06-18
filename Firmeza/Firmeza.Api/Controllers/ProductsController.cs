using AutoMapper;
using Firmeza.Api.DTOs.Products;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public ProductsController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    /// <summary>Lista productos con filtros opcionales.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? active)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        if (active.HasValue)
            query = query.Where(p => p.IsActive == active.Value);

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<ProductDto>>(products));
    }

    /// <summary>Obtiene un producto por ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        return Ok(_mapper.Map<ProductDto>(product));
    }

    /// <summary>Devuelve las categorías disponibles.</summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var cats = await _db.Products.AsNoTracking()
            .Where(p => !string.IsNullOrEmpty(p.Category))
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return Ok(cats);
    }

    /// <summary>Crea un nuevo producto. [Admin]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductCreateDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            _mapper.Map<ProductDto>(product));
    }

    /// <summary>Actualiza campos de un producto. [Admin]</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] ProductUpdateDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (dto.Name is not null)        product.Name        = dto.Name;
        if (dto.Description is not null) product.Description = dto.Description;
        if (dto.Category is not null)    product.Category    = dto.Category;
        if (dto.Unit is not null)        product.Unit        = dto.Unit;
        if (dto.Price.HasValue)          product.Price       = dto.Price.Value;
        if (dto.Stock.HasValue)          product.Stock       = dto.Stock.Value;
        if (dto.IsActive.HasValue)       product.IsActive    = dto.IsActive.Value;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(_mapper.Map<ProductDto>(product));
    }

    /// <summary>Elimina un producto. [Admin]</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        var inUse = await _db.SaleDetails.AnyAsync(d => d.ProductId == id);
        if (inUse)
            return Conflict(new { message = "El producto está referenciado en ventas y no puede eliminarse." });

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
