using Firmeza.Api.DTOs.Auth;
using Firmeza.Api.Services;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwt;
    private readonly IEmailService _email;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtService jwt,
        IEmailService email)
    {
        _userManager = userManager;
        _jwt = jwt;
        _email = email;
    }

    /// <summary>Obtiene un token JWT para el usuario autenticado.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Credenciales inválidas." });

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwt.GenerateToken(user, roles);

        return Ok(new TokenResponseDto
        {
            Token    = token,
            ExpiresAt = expiresAt,
            Email    = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles    = roles,
        });
    }

    /// <summary>Registra un nuevo usuario con rol Cliente.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Conflict(new { message = "El correo ya está registrado." });

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

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "Cliente");

        _ = _email.SendWelcomeAsync(user.Email!, $"{user.FirstName} {user.LastName}");

        var (token, expiresAt) = _jwt.GenerateToken(user, ["Cliente"]);
        return CreatedAtAction(nameof(Login), new TokenResponseDto
        {
            Token    = token,
            ExpiresAt = expiresAt,
            Email    = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles    = ["Cliente"],
        });
    }
}
