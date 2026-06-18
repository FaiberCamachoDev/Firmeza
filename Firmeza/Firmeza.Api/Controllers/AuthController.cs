using Firmeza.Api.DTOs.Auth;
using Firmeza.Api.Services;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwt;
    private readonly IEmailService _email;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtService jwt,
        IEmailService email,
        ApplicationDbContext db,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _jwt         = jwt;
        _email       = email;
        _db          = db;
        _logger      = logger;
    }

    /// <summary>Obtiene un token JWT y establece la cookie de sesión httpOnly.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Credenciales inválidas." });

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwt.GenerateToken(user, roles);

        // H5: cookie httpOnly — el token no es accesible desde JavaScript
        SetAuthCookie(token, expiresAt);

        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == dto.Email && c.IsActive);

        return Ok(new TokenResponseDto
        {
            Token      = token,
            ExpiresAt  = expiresAt,
            Email      = user.Email!,
            FullName   = $"{user.FirstName} {user.LastName}",
            Roles      = roles,
            CustomerId = customer?.Id,
        });
    }

    /// <summary>Registra un nuevo usuario con rol Cliente y crea su registro de cliente.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName       = dto.Email,
            Email          = dto.Email,
            FirstName      = dto.FirstName,
            LastName       = dto.LastName,
            DocumentNumber = dto.DocumentNumber,
            PhoneNumber    = dto.Phone,
            EmailConfirmed = true,
        };

        // H7: no pre-check — CreateAsync usa la restricción única de la BD (sin TOCTOU)
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
                return Conflict(new { message = "El correo ya está registrado." });
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // M2: si asignar el rol falla, eliminar el usuario para evitar estado inconsistente
        var roleResult = await _userManager.AddToRoleAsync(user, "Cliente");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            _logger.LogError("No se pudo asignar rol 'Cliente' al usuario {Email}: {Errors}",
                dto.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error interno al crear la cuenta. Intenta nuevamente." });
        }

        var existingCustomer = await _db.Customers
            .FirstOrDefaultAsync(c => c.DocumentNumber == dto.DocumentNumber);

        Customer customer;
        if (existingCustomer is null)
        {
            customer = new Customer
            {
                FirstName      = dto.FirstName,
                LastName       = dto.LastName,
                DocumentNumber = dto.DocumentNumber,
                Phone          = dto.Phone,
                Email          = dto.Email,
                Address        = string.Empty,
                IsActive       = true,
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
        }
        else
        {
            customer = existingCustomer;
        }

        // M4: email en background con log de error si falla
        _ = Task.Run(async () =>
        {
            try
            {
                await _email.SendWelcomeAsync(dto.Email, $"{dto.FirstName} {dto.LastName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de bienvenida a {Email}", dto.Email);
            }
        });

        var (token, expiresAt) = _jwt.GenerateToken(user, ["Cliente"]);
        SetAuthCookie(token, expiresAt);

        return CreatedAtAction(nameof(Login), new TokenResponseDto
        {
            Token      = token,
            ExpiresAt  = expiresAt,
            Email      = user.Email!,
            FullName   = $"{user.FirstName} {user.LastName}",
            Roles      = ["Cliente"],
            CustomerId = customer.Id,
        });
    }

    /// <summary>Cierra la sesión limpiando la cookie httpOnly.</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("firmeza_auth", new CookieOptions { Path = "/" });
        return NoContent();
    }

    private void SetAuthCookie(string token, DateTime expiresAt)
    {
        Response.Cookies.Append("firmeza_auth", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = !HttpContext.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase),
            SameSite = SameSiteMode.Lax,
            Expires  = expiresAt,
            Path     = "/",
        });
    }
}
