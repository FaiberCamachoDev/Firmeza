using Firmeza.Api.Services;
using Firmeza.Web.Models;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Firmeza.Tests.Api;

public class JwtServiceTests
{
    private static JwtService BuildService(int expireHours = 8)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]         = "test-super-secret-key-min-32-chars-ok",
                ["Jwt:Issuer"]      = "Firmeza.Test",
                ["Jwt:Audience"]    = "Firmeza.Clients",
                ["Jwt:ExpireHours"] = expireHours.ToString(),
            })
            .Build();

        return new JwtService(config);
    }

    private static ApplicationUser TestUser() => new()
    {
        Id        = Guid.NewGuid().ToString(),
        Email     = "test@firmeza.com",
        UserName  = "test@firmeza.com",
        FirstName = "Juan",
        LastName  = "Pérez",
    };

    [Fact]
    public void GenerateToken_ReturnsNonEmptyToken()
    {
        var svc = BuildService();
        var (token, _) = svc.GenerateToken(TestUser(), ["Admin"]);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ExpiresAt_IsCorrectOffset()
    {
        var svc = BuildService(expireHours: 4);
        var before = DateTime.UtcNow;
        var (_, expiresAt) = svc.GenerateToken(TestUser(), []);
        var after = DateTime.UtcNow;

        Assert.InRange(expiresAt, before.AddHours(4).AddSeconds(-5), after.AddHours(4).AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var svc = BuildService();
        var user = TestUser();
        var (token, _) = svc.GenerateToken(user, []);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }

    [Fact]
    public void GenerateToken_ContainsSubClaim_WithUserId()
    {
        var svc = BuildService();
        var user = TestUser();
        var (token, _) = svc.GenerateToken(user, []);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
    }

    [Fact]
    public void GenerateToken_ContainsRoleClaims()
    {
        var svc = BuildService();
        var (token, _) = svc.GenerateToken(TestUser(), ["Admin", "Cliente"]);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roles = jwt.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        Assert.Contains("Admin",   roles);
        Assert.Contains("Cliente", roles);
    }

    [Fact]
    public void GenerateToken_HasUniqueJti_PerCall()
    {
        var svc = BuildService();
        var user = TestUser();
        var (token1, _) = svc.GenerateToken(user, []);
        var (token2, _) = svc.GenerateToken(user, []);

        var handler = new JwtSecurityTokenHandler();
        var jti1 = handler.ReadJwtToken(token1).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = handler.ReadJwtToken(token2).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public void GenerateToken_ContainsFirstAndLastNameClaims()
    {
        var svc = BuildService();
        var user = TestUser();
        var (token, _) = svc.GenerateToken(user, []);

        var handler = new JwtSecurityTokenHandler();
        var claims = handler.ReadJwtToken(token).Claims.ToList();

        Assert.Contains(claims, c => c.Type == "firstName" && c.Value == user.FirstName);
        Assert.Contains(claims, c => c.Type == "lastName"  && c.Value == user.LastName);
    }
}
