using AutoMapper;
using Firmeza.Api.DTOs.Customers;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CustomersController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    /// <summary>Lista clientes. [Admin]</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? active)
    {
        var query = _db.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term)  ||
                c.DocumentNumber.ToLower().Contains(term));
        }

        if (active.HasValue)
            query = query.Where(c => c.IsActive == active.Value);

        var customers = await query.OrderBy(c => c.LastName).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<CustomerDto>>(customers));
    }

    /// <summary>Obtiene un cliente por ID. [Admin]</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>Crea un cliente. [Admin]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerCreateDto dto)
    {
        var exists = await _db.Customers
            .AnyAsync(c => c.DocumentNumber == dto.DocumentNumber);
        if (exists)
            return Conflict(new { message = "Ya existe un cliente con ese número de documento." });

        var customer = _mapper.Map<Customer>(dto);
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = customer.Id },
            _mapper.Map<CustomerDto>(customer));
    }

    /// <summary>Actualiza un cliente. [Admin]</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] CustomerUpdateDto dto)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        if (dto.FirstName is not null) customer.FirstName = dto.FirstName;
        if (dto.LastName  is not null) customer.LastName  = dto.LastName;
        if (dto.Phone     is not null) customer.Phone     = dto.Phone;
        if (dto.Email     is not null) customer.Email     = dto.Email;
        if (dto.Address   is not null) customer.Address   = dto.Address;
        if (dto.IsActive.HasValue)     customer.IsActive  = dto.IsActive.Value;
        customer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>Desactiva (soft-delete) un cliente. [Admin]</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        customer.IsActive  = false;
        customer.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
