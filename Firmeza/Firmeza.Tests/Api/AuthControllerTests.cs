using Firmeza.Api.Controllers;
using Firmeza.Api.DTOs.Auth;
using Firmeza.Api.Services;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Firmeza.Tests.Api;

public class AuthControllerTests : IDisposable
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static JwtService BuildJwtService() => new JwtService(
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]         = "test-super-secret-key-min-32-chars-ok",
                ["Jwt:Issuer"]      = "Firmeza.Test",
                ["Jwt:Audience"]    = "Firmeza.Clients",
                ["Jwt:ExpireHours"] = "8",
            })
            .Build());

    private static Mock<UserManager<ApplicationUser>> BuildUserManagerMock() =>
        new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
#pragma warning disable CS8625
            null, null, null, null, null, null, null, null
#pragma warning restore CS8625
        );

    private static Mock<IEmailService> BuildEmailMock()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(e => e.SendWelcomeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static ApplicationUser MakeUser(string email = "ana@firmeza.com") => new()
    {
        Id             = Guid.NewGuid().ToString(),
        Email          = email,
        UserName       = email,
        FirstName      = "Ana",
        LastName       = "López",
        DocumentNumber = "99999999",
        EmailConfirmed = true,
    };

    // Cada test usa su propia base en memoria para evitar colisiones
    private readonly ApplicationDbContext _db;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    // -----------------------------------------------------------------------
    // Login
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var user = MakeUser();
        var um   = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        um.Setup(m => m.CheckPasswordAsync(user, "Pass123!")).ReturnsAsync(true);
        um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["Cliente"]);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var result = await controller.Login(new LoginDto { Email = user.Email!, Password = "Pass123!" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<TokenResponseDto>(ok.Value);
        Assert.False(string.IsNullOrWhiteSpace(dto.Token));
        Assert.Equal(user.Email, dto.Email);
        Assert.Equal("Ana López", dto.FullName);
        Assert.Contains("Cliente", dto.Roles);
    }

    [Fact]
    public async Task Login_UserNotFound_Returns401()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var result = await controller.Login(new LoginDto { Email = "ghost@firmeza.com", Password = "any" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var user = MakeUser();
        var um   = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        um.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var result = await controller.Login(new LoginDto { Email = user.Email!, Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithExistingCustomer_ReturnsCustomerId()
    {
        var user = MakeUser("linked@firmeza.com");
        var customer = new Customer
        {
            FirstName = "Ana", LastName = "López",
            DocumentNumber = "11111111", Email = "linked@firmeza.com",
            Phone = "000", IsActive = true,
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        var um = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        um.Setup(m => m.CheckPasswordAsync(user, "Pass123!")).ReturnsAsync(true);
        um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["Cliente"]);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var result = await controller.Login(new LoginDto { Email = "linked@firmeza.com", Password = "Pass123!" });

        var ok  = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<TokenResponseDto>(ok.Value);
        Assert.Equal(customer.Id, dto.CustomerId);
    }

    // -----------------------------------------------------------------------
    // Register
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Register_NewEmail_Returns201WithToken()
    {
        var um    = BuildUserManagerMock();
        var email = BuildEmailMock();

        um.Setup(m => m.FindByEmailAsync("nuevo@firmeza.com")).ReturnsAsync((ApplicationUser?)null);
        um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
          .ReturnsAsync(IdentityResult.Success);
        um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Cliente"))
          .ReturnsAsync(IdentityResult.Success);

        var controller = new AuthController(um.Object, BuildJwtService(), email.Object, _db);

        var dto = new RegisterDto
        {
            Email          = "nuevo@firmeza.com",
            Password       = "Pass123!",
            FirstName      = "Carlos",
            LastName       = "Ríos",
            DocumentNumber = "12345678",
            Phone          = "999000111",
        };

        var result = await controller.Register(dto);

        var created  = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(created.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("nuevo@firmeza.com", response.Email);
        Assert.Contains("Cliente", response.Roles);
    }

    [Fact]
    public async Task Register_NewEmail_CreatesCustomerRecord()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
          .ReturnsAsync(IdentityResult.Success);
        um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Cliente"))
          .ReturnsAsync(IdentityResult.Success);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var dto = new RegisterDto
        {
            Email = "cliente@firmeza.com", Password = "Pass123!",
            FirstName = "Laura", LastName = "Gómez",
            DocumentNumber = "87654321", Phone = "111222333",
        };

        var result = await controller.Register(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);

        var createdCustomer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
        Assert.NotNull(createdCustomer);
        Assert.Equal(dto.FirstName, createdCustomer.FirstName);
        Assert.Equal(dto.DocumentNumber, createdCustomer.DocumentNumber);
    }

    [Fact]
    public async Task Register_NewEmail_ReturnsCustomerId()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
          .ReturnsAsync(IdentityResult.Success);
        um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Cliente"))
          .ReturnsAsync(IdentityResult.Success);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var dto = new RegisterDto
        {
            Email = "cid@firmeza.com", Password = "Pass123!",
            FirstName = "Pedro", LastName = "Ruiz",
            DocumentNumber = "55555555", Phone = "0",
        };

        var result = await controller.Register(dto);

        var created  = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(created.Value);
        Assert.NotNull(response.CustomerId);
        Assert.True(response.CustomerId > 0);
    }

    [Fact]
    public async Task Register_EmailAlreadyTaken_Returns409()
    {
        var existing = MakeUser("dup@firmeza.com");
        var um       = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync("dup@firmeza.com")).ReturnsAsync(existing);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var result = await controller.Register(new RegisterDto
        {
            Email = "dup@firmeza.com", Password = "Pass123!",
            FirstName = "X", LastName = "Y", DocumentNumber = "0", Phone = "0",
        });

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400WithErrors()
    {
        var um = BuildUserManagerMock();
        um.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var identityFailure = IdentityResult.Failed(
            new IdentityError { Code = "PasswordTooShort", Description = "La contraseña es muy corta." });

        um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
          .ReturnsAsync(identityFailure);

        var controller = new AuthController(um.Object, BuildJwtService(), BuildEmailMock().Object, _db);

        var result = await controller.Register(new RegisterDto
        {
            Email = "x@firmeza.com", Password = "123",
            FirstName = "X", LastName = "Y", DocumentNumber = "0", Phone = "0",
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
